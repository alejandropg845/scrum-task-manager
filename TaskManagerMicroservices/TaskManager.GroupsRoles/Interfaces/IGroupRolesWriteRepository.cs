using MongoDB.Driver;
using System.Threading.Tasks;
using TaskManager.Common.DTOs;
using TaskManager.GroupsRoles.Responses;

namespace TaskManager.GroupsRoles.Interfaces
{
    public interface IGroupRolesWriteRepository
    {
        Task<string> RemoveGroupsRolesAsync(string groupName);
        Task<Common.Documents.GroupsRoles> CreateGroupRoleAsync(string username, string roleName, string groupName, IClientSessionHandle? transaction);
        Task<string?> ProductOwnerAssignesOwnRoleAsync(string username, string groupName, string roleName, IClientSessionHandle transaction);
        Task<Common.Documents.GroupsRoles> UpdateGroupRoleAsync(string username, string groupName, string roleName, IClientSessionHandle? transaction);
        Task<Common.Documents.GroupsRoles> RemoveScrumMasterAndSetToOtherAsync
        (
            string? userGroupRole,
            string oldUsername,
            string newUsername,
            string groupName,
            string roleName,
            IClientSessionHandle transaction
        );
        Task DeleteUserGroupRoleAsync(string username, string groupName);
    }
}
