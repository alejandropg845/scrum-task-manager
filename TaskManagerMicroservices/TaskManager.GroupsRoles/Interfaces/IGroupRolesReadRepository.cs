using TaskManager.Common.DTOs;

namespace TaskManager.GroupsRoles.Interfaces
{
    public interface IGroupRolesReadRepository
    {
        Task<List<GroupRoleDto>> GetUsersGroupRolesAsync(string groupName);
        Task<string?> GetGroupRoleNameAsync(string groupName, string username);
        Task<bool> ProductOwnerExistsAsync(string groupName);
        Task<bool> UserIsProductOwnerAsync(string groupName, string currentUser);
        Task<Common.Documents.GroupsRoles?> GetScrumMasterAsync(string groupName);
    }
}
