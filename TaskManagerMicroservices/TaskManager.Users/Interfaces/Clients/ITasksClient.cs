using System.Text.Json;

namespace TaskManager.Users.Interfaces.Clients
{
    public interface ITasksClient
    {
        Task<JsonDocument?> MarkSprintTasksAsFinishedAsync(string token, string sprintId);
    }
}
