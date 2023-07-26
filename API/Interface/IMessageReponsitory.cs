using API.Entities;
using API.Helpers;
using Newtonsoft.Json.Linq;
using API.DTOs;

namespace API.Interface
{
    public interface IMessageReponsitory
    {
        void AddMessage(Message messages);
        void DeletedMessage(Message message);
        Task<Message> GetMessage(int id);
        Task<PagedList<MessageDTO>> GetMessagesForUser(MessageParams messageParams);
        Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUserName, string recipientUserName);
        Task<bool> SaveAllAsync();

    }
}