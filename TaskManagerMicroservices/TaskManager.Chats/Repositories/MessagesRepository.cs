using MongoDB.Driver;
using TaskManager.Chats.Interfaces;
using TaskManager.Common.Documents;

namespace TaskManager.Chats.Repositories
{
    public class MessagesRepository : IMessagesRepository
    {
        private readonly IMongoCollection<Message> _messagesCollection;
        private readonly FilterDefinitionBuilder<Message> _filter = Builders<Message>.Filter;

        public MessagesRepository(IMongoCollection<Message> messagesCollection)
        {
            _messagesCollection = messagesCollection;
        }

        public async Task<Message> AddMessageAsync(Message message, IClientSessionHandle? transaction)
        {
            if (transaction is not null)
                await _messagesCollection.InsertOneAsync(transaction, message);
            else
                await _messagesCollection.InsertOneAsync(message);
            return message;
        }

        public async Task<List<Message>> GetDateMessagesAsync(string dateId, int messagesPage, int sentMessages)
        {

            const int pageSize = 10;

            var messages = await _messagesCollection
                .Find(_filter.Eq(m => m.DateId, dateId))
                /* Ordenamos de manera descendente para que la base de datos ubique de primeros los
                 * mensajes mas recientes al mas viejo*/
                .SortByDescending(m => m.MessageTime)

                /* Tomamos dependiendo de la pagina, siendo 0 por default
                 por lo que no va a skipear ningun mensaje al principio*/
                .Skip((messagesPage * pageSize) + sentMessages)

                /* Limitamos a obtener los primeros 10 únicamente de la base de datos */
                .Limit(pageSize)
                .ToListAsync();


            /* Hacer reverse a los messages, de esta manera ubicamos los mas recientes al
             * final como suelen hacerlo los chats*/
            messages.Reverse();

            return messages;
        }
    }
}
