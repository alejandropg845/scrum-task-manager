using MongoDB.Driver;
using TaskManager.Common.DTOs;
using TaskManager.GroupsRoles.Interfaces;
using TaskManager.Common.Documents;
using TaskManager.GroupsRoles.Responses;
using System.Transactions;
using System.Data.Common;
using static Google.Apis.Requests.BatchRequest;

namespace TaskManager.GroupsRoles.Repositories
{
    public class GroupsRolesRepository : IGroupRolesWriteRepository, IGroupRolesReadRepository
    {
        private readonly IMongoCollection<Common.Documents.GroupsRoles> _groupsRolesCollection;
        private readonly FilterDefinitionBuilder<Common.Documents.GroupsRoles> _fb = Builders<Common.Documents.GroupsRoles>.Filter;
        public GroupsRolesRepository(IMongoCollection<Common.Documents.GroupsRoles> collection)
        {
            _groupsRolesCollection = collection;
        }
        public async Task<List<GroupRoleDto>> GetUsersGroupRolesAsync(string groupName)
        {
            var filter = _fb.Eq(gr => gr.GroupName, groupName);

            var projection = Builders<Common.Documents.GroupsRoles>
                .Projection.Expression(gr => new GroupRoleDto(gr.UserName, gr.RoleName));

            return await _groupsRolesCollection.Find(filter).Project(projection).ToListAsync();
        }

        public async Task<string?> GetGroupRoleNameAsync(string groupName, string username)
        {
            var filter = _fb.And
            (
                _fb.Eq(gr => gr.UserName, username),
                _fb.Eq(gr => gr.GroupName, groupName)
            );

            var projection = Builders<Common.Documents.GroupsRoles>.Projection.Expression(gr => gr.RoleName);

            string? userGroupRoleName = await _groupsRolesCollection
                .Find(filter).Project(projection).FirstOrDefaultAsync();

            return userGroupRoleName;

        }
        public async Task<Common.Documents.GroupsRoles?> GetScrumMasterAsync(string groupName)
        {
            var filterScrum = _fb.And
            (
                _fb.Eq(gr => gr.GroupName, groupName),
                _fb.Eq(gr => gr.RoleName, "scrum master")
            );
            var scrumMasterRole = await _groupsRolesCollection.Find(filterScrum).FirstOrDefaultAsync();

            return scrumMasterRole;
        }
        public async Task<bool> ProductOwnerExistsAsync(string groupName)
        {
            var filter = _fb.And
            (
                _fb.Eq(gr => gr.GroupName, groupName),
                _fb.Eq(gr => gr.RoleName, "product owner")
            );

            return await _groupsRolesCollection.Find(filter).AnyAsync();
        }

        public async Task<bool> UserIsProductOwnerAsync(string groupName, string currentUser)
        {
            var filter = _fb.And
            (
                _fb.Eq(gr => gr.GroupName, groupName),
                _fb.Eq(gr => gr.UserName, currentUser),
                _fb.Eq(gr => gr.RoleName, "product owner")
            );
            /* Verificar que el currentUser es product owner */
            return await _groupsRolesCollection.Find(filter).AnyAsync();
        }
        public async Task<string?> ProductOwnerAssignesOwnRoleAsync(string username, string groupName, string roleName, IClientSessionHandle transaction)
        {

            /* El usuario está asignando su rol de product owner a otro usuario */
            bool assigningProductOwner = (roleName == "product owner");

            if (assigningProductOwner)
            {
                var filter = _fb.And
                (
                    _fb.Eq(gr => gr.UserName, username),
                    _fb.Eq(gr => gr.GroupName, groupName.Trim())
                );

                var update = Builders<Common.Documents.GroupsRoles>
                    .Update.Set(gr => gr.RoleName, "none");

                await _groupsRolesCollection
                    .UpdateOneAsync(transaction, filter, update);

                return username;
            }

            return null;
        }
        public async Task<Common.Documents.GroupsRoles> RemoveScrumMasterAndSetToOtherAsync(string? userGroupRole,
            string oldUsername, string newUsername, string groupName, string roleName, IClientSessionHandle transaction)
        {
            if (userGroupRole is not null)
                // El usuario a cambiarle el role ya contaba con un role, entonces hacemos update
                await UpdateGroupRoleAsync(newUsername, groupName, roleName, transaction);
            else
                // No existe ningun role entonces creamos uno
                await CreateGroupRoleAsync(newUsername, roleName, groupName, transaction);

            // Cambiamos el role del usuario viejo
            await UpdateGroupRoleAsync(
                oldUsername,
                groupName,
                "none",
                transaction
            );

            var groupRole = new Common.Documents.GroupsRoles
            {
                GroupName = groupName,
                RoleName = roleName,
                UserName = newUsername
            };

            return groupRole;
        }
        public async Task<Common.Documents.GroupsRoles> UpdateGroupRoleAsync(string username, string groupName, string roleName, IClientSessionHandle? transaction)
        {
            var filter = _fb.And
            (
                _fb.Eq(gr => gr.GroupName, groupName),
                _fb.Eq(gr => gr.UserName, username)
            );

            var update = Builders<Common.Documents.GroupsRoles>.Update.Set(gr => gr.RoleName, roleName);

            if (transaction is not null)
                await _groupsRolesCollection.UpdateOneAsync(transaction, filter, update);
            else
                await _groupsRolesCollection.UpdateOneAsync(filter, update);

            return new Common.Documents.GroupsRoles
            {
                GroupName = groupName,
                RoleName = roleName,
                UserName = username
            };
        }
        public async Task<Common.Documents.GroupsRoles> CreateGroupRoleAsync(string username, string roleName, string groupName, IClientSessionHandle? transaction)
        {
            var newGroupRole = new Common.Documents.GroupsRoles
            {
                GroupName = groupName,
                RoleName = roleName,
                UserName = username,
                Id = Guid.NewGuid().ToString()
            };

            if (transaction is null)
                await _groupsRolesCollection.InsertOneAsync(newGroupRole);
            else
                await _groupsRolesCollection.InsertOneAsync(transaction, newGroupRole);

            return newGroupRole;
        }

        public async Task<string> RemoveGroupsRolesAsync(string groupName)
        {

            var filter = _fb.Eq(gr => gr.GroupName, groupName.Trim());

            var result = await _groupsRolesCollection.DeleteManyAsync(filter);

            return groupName;
        }
        public async Task DeleteUserGroupRoleAsync(string username, string groupName)
        {
            var filter = _fb.And
            (
                _fb.Eq(gr => gr.UserName, username),
                _fb.Eq(gr => gr.GroupName, groupName)
            );

            var result = await _groupsRolesCollection.DeleteOneAsync(filter);
        }
    }
}
