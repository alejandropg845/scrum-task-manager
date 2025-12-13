using MongoDB.Driver;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;

namespace TaskManager.Chats.Interfaces
{
    public interface IDatesRepository
    {
        Task<MessagesDate?> GetMessagesDateAsync(string groupName);
        Task<MessagesDate?> GetNextDateAsync(string groupName, int datePage); 
        Task<DateTime?> GetDateFullDateTimeAsync(string dateId);
        Task AddMessagesDateAsync(MessagesDate messagesDate, IClientSessionHandle transaction);
    }
}
