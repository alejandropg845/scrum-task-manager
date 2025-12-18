using MongoDB.Driver;
using TaskManager.Common.Documents;

namespace TaskManager.Chats.Interfaces
{
    public interface IMessagesService
    {
        Task<List<Message>> GetDateMessagesAsync(string dateId, int messagesPage, int sentMessages);
        Task<Message> AddMessageAsync(Message message, IClientSessionHandle? transaction);
    }
}
