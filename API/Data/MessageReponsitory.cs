
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
      public class MessageReponsitory : IMessageReponsitory
      {
            private readonly DataContext _context;
            public readonly IMapper _mapper;
            public MessageReponsitory(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }
            public void AddMessage(Message messages)
            {
               _context.Messages.Add(messages);
            }

            public void DeletedMessage(Message message)
            {
                _context.Messages.Remove(message);
            }

            public async Task<Message> GetMessage(int id)
            {
                return await _context.Messages.FindAsync(id);
            }

            public async Task<PagedList<MessageDTO>> GetMessagesForUser(MessageParams messageParams)
            {
                var query = _context.Messages
                    .OrderByDescending(x => x.MessageSent)
                    .AsQueryable();

                query = messageParams.Container switch
                {
                    "Inbox" => query.Where(u => u.RecipientUsername == messageParams.Username 
                        && u.RecipientDeleted == false),
                    "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username  
                        && u.SenderDeleted == false),
                    _ => query.Where(u => u.RecipientUsername == messageParams.Username 
                        && u.RecipientDeleted == false && u.DateRead == null)
                };

                var messages = query.ProjectTo<MessageDTO>(_mapper.ConfigurationProvider);

                return await PagedList<MessageDTO>
                            .CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
            }

            public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUserName, string recipientUserName)
            {
                var query = _context.Messages
                    .Where(
                        m => m.RecipientUsername == currentUserName && m.RecipientDeleted == false &&
                        m.SenderUsername == recipientUserName ||
                        m.RecipientUsername == recipientUserName && m.SenderDeleted == false &&
                        m.SenderUsername == currentUserName 
                    )
                    .OrderBy(m => m.MessageSent)
                    .AsQueryable();
                    

                var unreadMessages = query.Where(m => m.DateRead == null 
                && m.RecipientUsername == currentUserName).ToList();

                if(unreadMessages.Any())
                {
                    foreach (var message in unreadMessages)
                    {
                        message.DateRead = DateTime.UtcNow;
                    }

                    // await _context.SaveChangesAsync();
                }

                return await query.ProjectTo<MessageDTO>(_mapper.ConfigurationProvider).ToListAsync();

            }

            // public async Task<bool> SaveAllAsync()
            // {
            //     return await _context.SaveChangesAsync() > 0;
            // }

            public void AddGroup(Group group)
            {
                _context.Groups.Add(group);
            }

            public async Task<Connection> GetConnection(string connectionId)
            {
                return await _context.Connections.FindAsync(connectionId);
            }

            public async Task<Group> GetMessageGroup(string groupName)
            {
                return await _context.Groups
                    .Include(x => x.Connections)
                    .FirstOrDefaultAsync(x => x.Name == groupName);
            }

            public void RemoveConnection(Connection connection)
            {
                _context.Connections.Remove(connection);
            }

            public async Task<Group> GetGroupForConnection(string connectionId)
            {
                return await _context.Groups
                    .Include(x => x.Connections)
                    .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId))
                    .FirstOrDefaultAsync();
            }
      }
}