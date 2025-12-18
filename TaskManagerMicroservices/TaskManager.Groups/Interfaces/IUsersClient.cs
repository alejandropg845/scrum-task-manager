using System.Text.Json;

namespace TaskManager.Groups.Interfaces
{
    public interface IUsersClient
    {
        Task<JsonDocument?> SetGroupToUserAsync(string groupName, string token, object? body);
        Task DeleteGroupForMembersAsync(string groupName, string token);
    }
}
