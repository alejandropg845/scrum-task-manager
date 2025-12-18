using MongoDB.Driver;
using TaskManager.Chats.Interfaces;
using TaskManager.Common.Configurations;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Common.Interfaces;

namespace TaskManager.Chats.Repositories
{
    public class DatesRepository : IDatesRepository
    {
        private readonly FilterDefinitionBuilder<MessagesDate> _filter = Builders<MessagesDate>.Filter;
        private readonly IMongoCollection<MessagesDate> _dateCollection;
        public DatesRepository(IMongoCollection<MessagesDate> collection)
        {
            _dateCollection = collection;
        }
        public async Task<DateTime?> GetDateFullDateTimeAsync(string dateId)
        {
            var filter = _filter.Eq(d => d.Id, dateId);

            var projection = Builders<MessagesDate>.Projection.Expression(d => d.MessagesFullDateInfo);

            var date = await _dateCollection
                .Find(filter).Project(projection)
                .FirstOrDefaultAsync();

            return date;
        }
        public async Task<MessagesDate?> GetNextDateAsync(string groupName, int datePage)
        {
            var datesFilter = _filter.Eq(md => md.GroupName, groupName);

            const int pageSize = 1;

            var date = await _dateCollection
                /* Ordenar de mayor a menor */
                .Find(datesFilter).SortByDescending(d => d.MessagesFullDateInfo)
                /* Descartar date (uno solo)*/
                .Skip(datePage * pageSize)
                /* Tomar el siguiente */
                .Limit(1)
                .FirstOrDefaultAsync();

            return date;
        }
        public async Task AddMessagesDateAsync(MessagesDate messagesDate, IClientSessionHandle transaction)
        => await _dateCollection.InsertOneAsync(transaction, messagesDate);
        public async Task<MessagesDate?> GetMessagesDateAsync(string groupName)
        {
            var filter = _filter.Eq(d => d.GroupName, groupName);

            return await _dateCollection
            .Find(filter).SortByDescending(d => d.MessagesFullDateInfo)
            .Limit(1).FirstOrDefaultAsync();
        }

    }
}
