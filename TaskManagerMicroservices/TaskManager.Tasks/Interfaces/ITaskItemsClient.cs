using System.Text.Json;
using TaskManager.Common.Documents;

namespace TaskManager.Tasks.Interfaces
{
    public interface ITaskItemsClient
    {
        Task<JsonDocument?> GetTaskItemsAsync(string taskId, string token);
    }
}
