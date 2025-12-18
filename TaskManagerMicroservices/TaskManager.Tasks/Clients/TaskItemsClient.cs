using Newtonsoft.Json.Linq;
using System.Text.Json;
using TaskManager.Common.Documents;
using TaskManager.Common.Interfaces;
using TaskManager.Tasks.Interfaces;

namespace TaskManager.Tasks.Clients
{
    public class TaskItemsClient : ITaskItemsClient
    {
        private readonly IAuthenticationClient _authenticationClient;
        public TaskItemsClient(IAuthenticationClient auth)
        {
            _authenticationClient = auth;
        }
        public Task<JsonDocument?> GetTaskItemsAsync(string taskId, string token)
        {
            return _authenticationClient
            .SendRequestAsync(
                "task-items", 
                $"GetTaskItems/{taskId}", 
                "get", 
                token, 
                null
            );
        }
    }
}
