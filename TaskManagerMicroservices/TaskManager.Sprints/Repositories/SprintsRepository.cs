using MongoDB.Driver;
using System.Data.Common;
using System.Globalization;
using System.Transactions;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Common.Interfaces;
using TaskManager.Sprints.Interfaces;
using TaskManager.Sprints.Responses;

namespace TaskManager.Sprints.Repositories
{
    public class SprintsRepository : ISprintWriteRepository, ISprintReadRepository
    {
        private readonly IMongoCollection<Common.Documents.Sprint> _sprintCollection;
        private readonly FilterDefinitionBuilder<Common.Documents.Sprint> _fb = Builders<Common.Documents.Sprint>.Filter;
        public SprintsRepository(IMongoCollection<Sprint> collection)
        {
            _sprintCollection = collection;
        }

        public async Task<List<ToSprintDto>> GetGroupSprintsAsync(string groupName)
        {
            var filter = _fb.And
            (
                _fb.Eq(s => s.Status, "completed"),
                _fb.Eq(s => s.GroupName, groupName)
            );

            var projection = Builders<Sprint>.Projection
                .Expression(s => new ToSprintDto
                {
                    Status = s.Status,
                    ExpirationTime = s.SprintExpiration,
                    Id = s.Id,
                    SprintNumber = s.SprintNumber,
                    SprintName = s.SprintName
                });

            return await _sprintCollection.Find(filter).Project(projection).ToListAsync();

        }
        
        public Task<ToSprintDto> GetCurrentSprintAsync(string groupName)
        {
            var filter = _fb.And
           (
               _fb.Eq(s => s.GroupName, groupName),
               _fb.Or
               (
                   _fb.Eq(s => s.Status, "created"),
                   _fb.Eq(s => s.Status, "begun")
               )
           );

            var projection = Builders<Sprint>.Projection
                .Expression(sprint => new ToSprintDto
                {
                    ExpirationTime = sprint.SprintExpiration,
                    Status = sprint.Status,
                    Id = sprint.Id,
                    SprintNumber = sprint.SprintNumber,
                    SprintName = sprint.SprintName
                });

            return _sprintCollection.Find(filter)
                .Project(projection)
                .FirstOrDefaultAsync();
        }
        public async Task<string?> GetPreviousSprintIdAsync(string groupName)
        {
            var filter = _fb.Eq(s => s.GroupName, groupName);
            var projection = Builders<Sprint>.Projection.Expression(s => s.Id);

            string? previousSprintId = await _sprintCollection
                                    .Find(filter)
                                    .SortByDescending(s => s.SprintNumber)
                                    .Project(projection)
                                    .Skip(1).Limit(1)
                                    .FirstOrDefaultAsync();

            return previousSprintId;
        }
        public async Task AddSprintAsync(Sprint sprint, IClientSessionHandle transaction)
        => await _sprintCollection.InsertOneAsync(transaction, sprint);
        public async Task<ToSprintDto> CreateSprintAsync(string sprintId, string groupName, int sprintNumber)
        {
            var newSprint = new Sprint()
            {
                GroupName = groupName,
                Id = sprintId,
                SprintExpiration = null,
                Status = "created",
                SprintNumber = sprintNumber
            };

            await _sprintCollection.InsertOneAsync(newSprint);

            return new ToSprintDto
            {
                ExpirationTime = newSprint.SprintExpiration,
                Status = newSprint.Status,
                SprintNumber = sprintNumber,
                Id = newSprint.Id
            };

        }

        public async Task<int> GetSprintNumberAsync(string groupName)
        {
            var filter = _fb.Eq(s => s.GroupName, groupName);

            int sprintQuantity = (int) await _sprintCollection.CountDocumentsAsync(filter);

            return sprintQuantity + 1;
        }
        public async Task<Sprint> BeginSprintAsync(BeginSprintDto dto, DateTimeOffset expirationTime)
        {
            var filter = _fb.And
            (
                _fb.Eq(s => s.GroupName, dto.GroupName),
                _fb.Eq(s => s.Status, "created")
            );

            var update = Builders<Sprint>.Update
                .Set(s => s.SprintExpiration, expirationTime)
                .Set(s => s.Status, "begun")
                .Set(s => s.SprintName, dto.SprintName ?? null);


            var sprintBeforeUpdate = await _sprintCollection.FindOneAndUpdateAsync(filter, update,
            new FindOneAndUpdateOptions<Sprint>
            {
                ReturnDocument = ReturnDocument.Before
            });

            return sprintBeforeUpdate;
        }
        
        public async Task SetSprintAsCompletedAsync(string sprintId, IClientSessionHandle transaction)
        {
            var filter = _fb.And
            (
                _fb.Eq(s => s.Id, sprintId),
                _fb.Eq(s => s.Status, "begun")
            );


            var update = Builders<Sprint>.Update.Set(s => s.Status, "completed");

            await _sprintCollection.UpdateOneAsync(transaction, filter, update);
        }
        public async Task<int> DeleteSprintsAsync(string groupName)
        {

            var filter = _fb.Eq(s => s.GroupName, groupName);

            var result = await _sprintCollection.DeleteManyAsync(filter);

            return (int) result.DeletedCount;
        }

        public async Task<bool> CanMarkSprintTaskItemAsCompletedAsync(string sprintId)
        {
            var filter = _fb.And(
                _fb.Eq(s => s.Id, sprintId),
                _fb.Eq(s => s.Status, "begun")
            );

            return await _sprintCollection.Find(filter).AnyAsync();

        }

        public async Task<List<ToSprintDto>> GetCompletedSprintsForSummaryAsync(string groupName)
        {
            var filter = _fb.And(
                _fb.Eq(s => s.GroupName, groupName),
                _fb.Eq(s => s.Status, "completed")
            );

            var projection = Builders<Sprint>.Projection.Expression(s => new ToSprintDto
            {
                ExpirationTime = s.SprintExpiration,
                Id = s.Id,
                SprintName = s.SprintName,
                SprintNumber = s.SprintNumber,
                Status = s.Status
            });

            return await _sprintCollection.Find(filter).Project(projection).ToListAsync();

        }

        public async Task DeleteSprintAsync(string sprintId, IClientSessionHandle? transaction)
        {
            if (transaction != null)

                await _sprintCollection.DeleteOneAsync(transaction, _fb.Eq(s => s.Id, sprintId));

            else 
                await _sprintCollection.DeleteOneAsync(_fb.Eq(s => s.Id, sprintId));

        }

        public async Task RevertSprintStatusAsync(string sprintId, string groupName, string status, IClientSessionHandle? transaction)
        {
            var filter = _fb.And(
                _fb.Eq(s => s.Id, sprintId),
                _fb.Eq(s => s.GroupName, groupName)
            );

            var update = Builders<Sprint>.Update
                .Set(s => s.Status, status)
                .Set(s => s.SprintExpiration, null)
                .Set(s => s.SprintName, null);

            if (transaction != null)
                await _sprintCollection.UpdateOneAsync(transaction, filter, update);
            else
                await _sprintCollection.UpdateOneAsync(filter, update);
        }
    }
}
