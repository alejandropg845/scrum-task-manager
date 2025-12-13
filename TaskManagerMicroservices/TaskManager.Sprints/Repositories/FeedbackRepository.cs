using MongoDB.Driver;
using TaskManager.Common.Documents;
using TaskManager.Sprints.Interfaces;

namespace TaskManager.Sprints.Repositories
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly IMongoCollection<Feedback> _feedbackCollection;
        private readonly FilterDefinitionBuilder<Feedback> _filter = Builders<Feedback>.Filter;
        public FeedbackRepository(IMongoCollection<Feedback> collection)
        {
            _feedbackCollection = collection;
        }
        public async Task<(string? SprintId, bool IsSubmited)> IsFeedbackSubmitedAsync(string username, string groupName, string sprintId)
        {
            var filter = _filter.And
            (
                _filter.Eq(f => f.GroupName, groupName),
                _filter.Eq(f => f.Username, username),
                _filter.Eq(f => f.SprintId, sprintId)
            );

            var projection = Builders<Feedback>.Projection
                .Expression(f => new ValueTuple<string?, bool>(f.SprintId, f.IsSubmited));

            return await _feedbackCollection.Find(filter).Project(projection).FirstOrDefaultAsync();
           
        }


        public async Task<Feedback> AddFeedbackAsync(string groupName, string username, string sprintId)
        {
            var feedback = new Feedback
            {
                GroupName = groupName,
                Id = Guid.NewGuid().ToString(),
                IsSubmited = false,
                Username = username,
                SprintId = sprintId
            };

            await _feedbackCollection.InsertOneAsync(feedback);

            return feedback;
        }
        public async Task MarkFeedbackAsSubmitedAsync(string username, string groupName, string sprintId, IClientSessionHandle? transaction)
        {
            var filter = _filter.And(
                _filter.Eq(f => f.Username, username),
                _filter.Eq(f => f.GroupName, groupName),
                _filter.Eq(f => f.SprintId, sprintId)
            );

            var update = Builders<Feedback>.Update.Set(f => f.IsSubmited, true);

            if (transaction is null)
                await _feedbackCollection.UpdateOneAsync(filter, update);
            else
                await _feedbackCollection.UpdateOneAsync(transaction, filter, update);
        }
        public async Task<bool> DeleteFeedbackAsync(string feedbackId)
        {
            var filter = _filter.Eq(f => f.Id, feedbackId);

            await _feedbackCollection.DeleteOneAsync(filter);

            return true;
        }

        /* Saga */
        public async Task DeleteFeedbackToUsersAsync(string groupName, string sprintId)
        {
            var filter = _filter.And(
                _filter.Eq(r => r.SprintId, sprintId),
                _filter.Eq(r => r.GroupName, groupName)
            );

            await _feedbackCollection.DeleteManyAsync(filter);
        }
    }
}
