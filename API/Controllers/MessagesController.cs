using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class MessagesController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public MessagesController(IMapper mapper, IUnitOfWork uow)
        {
            _mapper = mapper;
            _uow = uow;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO)
        {
            var username = User.GetUserName();

            if(username == createMessageDTO.RecipientUsername.ToLower())
            {
                return BadRequest("You cannot send messages to yourself");
            }

            var sender = await _uow.UserRepository.GetUserByUserNameAsync(username);
            var recipient = await _uow.UserRepository.GetUserByUserNameAsync(createMessageDTO.RecipientUsername);

            if(recipient == null) return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDTO.Content
            };

            _uow.MessageReponsitory.AddMessage(message);

            if(await _uow.Complete()) return Ok(_mapper.Map<MessageDTO>(message));

            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDTO>>> GetMessagesForUser([FromQuery]
            MessageParams messageParams)
        {
            messageParams.Username = User.GetUserName();
            var messages = await  _uow.MessageReponsitory.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messageParams.PageSize,
                messages.TotalCount, messages.TotalPages));

            return messages;
        }

        // [HttpGet("thread/{username}")]
        // public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessageThread(string username)
        // {
        //     var currentUsername = User.GetUserName();

        //     return Ok(await  _uow.MessageReponsitory.GetMessageThread(currentUsername, username));
        // }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id) 
        {
            var username = User.GetUserName();

            var message = await  _uow.MessageReponsitory.GetMessage(id);

            if(message.SenderUsername != username && message.RecipientUsername !=username) return Unauthorized();

            if(message.SenderUsername == username) message.SenderDeleted = true;
            if(message.RecipientUsername == username) message.RecipientDeleted = true;

            if(message.SenderDeleted && message.RecipientDeleted)
            {
                 _uow.MessageReponsitory.DeletedMessage(message);
            }

            if(await _uow.Complete()) return Ok();
            
            return BadRequest("Problem deleting the message");
        }

    }
}