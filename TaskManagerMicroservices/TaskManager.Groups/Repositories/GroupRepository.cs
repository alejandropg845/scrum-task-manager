using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using System.Threading.Tasks;
using TaskManager.Common;
using TaskManager.Common.Configurations;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Common.Interfaces;
using TaskManager.Groups.Interfaces;

namespace TaskManager.Groups.Repositories
{
    public class GroupRepository : IGroupWriteRepository, IGroupReadRepository
    {
        private readonly IMongoCollection<Group> _groupsCollection;
        private readonly FilterDefinitionBuilder<Group> _filter = Builders<Group>.Filter;
        public GroupRepository(IMongoCollection<Group> collection)
        {
            _groupsCollection = collection;
        }
        public async Task<string?> KeepAliveAsync()
        {
            var filter = _filter.Eq(t => t.Name, "anygroupname43#$#");
            var projection = Builders<Group>.Projection.Expression(t => t.Name);

            return await _groupsCollection.Find(filter).Project(projection).FirstOrDefaultAsync();

        }
        
        public async Task<bool> GroupExistsAsync(string groupName)
        {
            var filter = _filter.Eq(g => g.Name, groupName);

            var groupExists = await _groupsCollection.Find(filter).AnyAsync();

            return groupExists;
        }

        public async Task<bool> IsGroupOwnerAsync(string username, string groupName)
        {
            var filter = _filter.And
                (
                    _filter.Eq(g => g.OwnerName, username),
                    _filter.Eq(g => g.Name, groupName)
                );

            bool userGroupName = await _groupsCollection
                .Find(filter).AnyAsync();

            return userGroupName;
        }
        public async Task<bool> DeleteGroupAsync(string groupName, string username)
        {
            var filter = _filter.And(
                    _filter.Eq(u => u.Name, groupName),
                    _filter.Eq(u => u.OwnerName, username)
            );

            await _groupsCollection.DeleteOneAsync(filter);

            return true;
        }
        
        public async Task<bool> IsScrumAsync(string groupName)
        {
            var filter = _filter.Eq(g => g.Name, groupName);

            var projection = Builders<Group>.Projection.Expression(g => g.IsScrum);

            bool isScrum = await _groupsCollection.Find(filter).Project(projection).FirstOrDefaultAsync();

            return isScrum;
        }
        public async Task<bool> SetAddingTasksAllowed(string username, string groupName, bool isAllowed)
        {

            var filter = _filter.And
            (
                _filter.Eq(g => g.Name, groupName),
                _filter.Eq(g => g.OwnerName, username)
            );

            var update = Builders<Group>.Update.Set(g => g.IsAddingTasksAllowed, isAllowed);

            await _groupsCollection.UpdateOneAsync(filter, update);

            return isAllowed;

        }

        public async Task<bool> IsAddingTasksAllowedAsync(string groupName)
        {
            var filter = _filter.Eq(g => g.Name, groupName);

            var projection = Builders<Group>.Projection.Expression(g => g.IsAddingTasksAllowed);

            return await _groupsCollection.Find(filter).Project(projection).FirstOrDefaultAsync();
        }
        public async Task<(bool IsScrum, bool IsAddingTasksAllowed, bool IsGroupOwner)> GetInitialGroupInfoAsync(string groupName, string username)
        {
            var filter = _filter.Eq(g => g.Name, groupName);

            var projection = Builders<Common.Documents.Group>
                .Projection.Expression(g => new ValueTuple<bool, bool, string>(g.IsScrum, g.IsAddingTasksAllowed, g.OwnerName));

            var response = await _groupsCollection.Find(filter).Project(projection).FirstOrDefaultAsync();

            bool isGroupOwner = (username == response.Item3);

            return new(response.Item1, response.Item2, isGroupOwner);

        }

        /* SAGA METHODS */
        public async Task DeleteCreatedGroupAsync(string groupName)
        {
            var filter = _filter.Eq(g => g.Name, groupName);


            var result = await _groupsCollection.DeleteOneAsync(filter);


        }
        public async Task<JsonDocument?> AddGroupAsync(string groupName, string username, bool isScrum)
        {

            var group = new Group
            {
                Id = Guid.NewGuid().ToString(),
                Name = groupName,
                OwnerName = username,
                IsScrum = isScrum
            };

            await _groupsCollection.InsertOneAsync(group);

            return null; /* <-- Recordemos que nuestro dictionary está esperando un tipo de dato
                          * Func<Task<JsonDocument?>>, por lo que podríamos convertir la variable group
                          * a un JsonDocument haciendo uso de JsonSerializer.SerializeToDocument, pero en
                          este caso como el valor no importa y simplemente queremos cumplir con el tipo de dato,
                          entonces podemos retornar null. */
        }
        public async Task<JsonDocument?> SetSprintIdToGroupAsync(string groupName, string sprintId)
        {

            var filter = _filter.Eq(g => g.Name, groupName);

            var update = Builders<Group>.Update.Set(g => g.SprintId, sprintId);

            var r = await _groupsCollection.UpdateOneAsync(filter, update);

            return null;
        }
        public async Task<bool> DeleteGroupSprintAsync(string groupName, string sprintId)
        {
            var filter = _filter.And(
                _filter.Eq(g => g.Name, groupName),
                _filter.Eq(g => g.SprintId, sprintId)
            );

            var update = Builders<Group>.Update.Set(g => g.SprintId, null);

            var result = await _groupsCollection.UpdateOneAsync(filter, update);

            return result.ModifiedCount == 1;
        }
    }
}
