using TaskManager.Common.DTOs;
using TaskManager.GroupsRoles.Responses;

namespace TaskManager.GroupsRoles.Interfaces
{
    public interface IGroupsRolesService
    {
        Task<SetGroupRoleResponse> SetGroupRoleAsync(SetUserGroupRoleDto dto, string username, string currentUser);
        Task<List<GroupRoleDto>> GetUsersGroupRolesAsync(string groupName);
        Task<string?> GetGroupRoleNameAsync(string groupName, string username);
        Task<string> RemoveGroupsRolesAsync(string groupName);
    }
}
