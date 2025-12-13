using System.Text.Json;

namespace TaskManager.Users.Interfaces.Clients
{
    public interface ISprintsClient
    {
        Task<JsonDocument?> CycleSprintAsync(string token, object? body);
    }
}
