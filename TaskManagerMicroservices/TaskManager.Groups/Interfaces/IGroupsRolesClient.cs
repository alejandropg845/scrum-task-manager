using System.Text.Json;

namespace TaskManager.Groups.Interfaces
{
    public interface IGroupsRolesClient
    {
        Task<string?> GetUserGroupRoleAsync(string groupName, string token, object? body);
        Task SetUserGroupRoleAsync(string groupName, string username, string token, object? body);
    }
}
