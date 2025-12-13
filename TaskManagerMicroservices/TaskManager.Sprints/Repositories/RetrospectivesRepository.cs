using MongoDB.Bson.IO;
using MongoDB.Driver;
using System.Text.Json;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Common.Interfaces;
using TaskManager.Sprints.Interfaces;

namespace TaskManager.Sprints.Repositories
{
    public class RetrospectivesRepository : IRetrospectivesRepository
    {
        private readonly IMongoCollection<SprintRetrospective> _retrospectivesCollection;
        private readonly FilterDefinitionBuilder<SprintRetrospective> _filter = Builders<SprintRetrospective>.Filter;
        
        public RetrospectivesRepository(IMongoCollection<SprintRetrospective> collection)
        {
            _retrospectivesCollection = collection;
        }
        public async Task<List<ToRetrospectiveDto>> GetRetrospectivesAsync(string groupName)
        {
            var filter = _filter.Eq(r => r.GroupName, groupName);

            var projection = Builders<SprintRetrospective>
                .Projection.Expression(r => new ToRetrospectiveDto
                (
                    r.Rating,
                    r.Feedback,
                    r.Name,
                    r.SubmitedAt
                ));

            return await _retrospectivesCollection.Find(filter).Project(projection).ToListAsync();
        }
        public async Task<ToRetrospectiveDto> AddSprintRetroAsync(CreateRetrospectiveDto dto, IClientSessionHandle? transaction)
        {
            var newRetro = new SprintRetrospective
            {
                Id = Guid.NewGuid().ToString(),
                SprintId = dto.SprintId,
                Feedback = dto.Feedback,
                GroupName = dto.GroupName,
                Name = dto.Name,
                Rating = dto.Rating,
                SubmitedAt = DateTimeOffset.UtcNow
            };

            if (transaction is not null)
                await _retrospectivesCollection.InsertOneAsync(transaction, newRetro);
            else
                await _retrospectivesCollection.InsertOneAsync(newRetro);

            return new ToRetrospectiveDto(newRetro.Rating, newRetro.Feedback, newRetro.Name, newRetro.SubmitedAt);
        }
    }
}
