using MongoDB.Driver;
using TaskManager.Chats.Interfaces;
using TaskManager.Common.Documents;

namespace TaskManager.Chats.Services
{
    public class MessagesService : IMessagesService
    {
        private readonly IMessagesRepository _repo;
        public MessagesService(IMessagesRepository repo)
        {
            _repo = repo;
        }

        public async Task<Message> AddMessageAsync(Message message, IClientSessionHandle transaction)
        {
            var addedMessage = await _repo.AddMessageAsync(message, transaction);

            return addedMessage;
        }

        public async Task<List<Message>> GetDateMessagesAsync(string dateId, int messagesPage, int sentMessages)
        => await _repo.GetDateMessagesAsync(dateId, messagesPage, sentMessages);

    }
}
