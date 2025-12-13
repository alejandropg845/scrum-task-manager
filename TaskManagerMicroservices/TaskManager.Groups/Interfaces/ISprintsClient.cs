using System.Text.Json;

namespace TaskManager.Groups.Interfaces
{
    public interface ISprintsClient
    {
        Task<JsonDocument> CreateSprintAsync(string sprintId, string groupName, string token, object? body);
    }
}
