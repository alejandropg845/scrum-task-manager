using System.Text.Json;

namespace TaskManager.Users.Interfaces.Clients
{
    public interface IGroupsRolesClient
    {
        Task<JsonDocument?> GetUsersGroupRolesAsync(string groupName, string token);
    }
}
