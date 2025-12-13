using TaskManager.Common.Interfaces;
using TaskManager.TaskItems.Interfaces;

namespace TaskManager.TaskItems.Clients
{
    public class TasksClient : ITasksClient
    {
        private readonly IAuthenticationClient _authenticationClient;
        public TasksClient(IAuthenticationClient client)
        {
            _authenticationClient = client;
        }
        public async Task<bool> TaskContainsSprintAsync(string taskId, string token)
        {
            var taskContainsSprint_JsonResponse = await _authenticationClient
            .SendRequestAsync(
                "tasks", 
                $"TaskContainsSprint/{taskId}", 
                "get", 
                token, 
                null
            );

            return taskContainsSprint_JsonResponse!.RootElement.GetProperty("result").GetBoolean();
        }
        public async Task<bool> TaskExistsAsync(string taskId, string token)
        {
            var jsonResponse = await _authenticationClient
            .SendRequestAsync(
                "tasks", 
                $"TaskExists/{taskId}", 
                "get", 
                token, 
                null
            );

            return jsonResponse!.RootElement.GetProperty("result").GetBoolean();
        }
        public async Task<bool> IsTaskOwnerAsync(string taskId, string token)
        {
            var jsonResponse = await _authenticationClient
            .SendRequestAsync(
                "tasks",
                $"IsTaskOwner/{taskId}",
                "get",
                token,
                null
            );

            return jsonResponse!.RootElement.GetProperty("result").GetBoolean();
        }
    }
}
