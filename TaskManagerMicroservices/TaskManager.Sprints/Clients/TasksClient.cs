using System.Text.Json;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Common.Interfaces;
using TaskManager.Sprints.Interfaces;

namespace TaskManager.Sprints.Clients
{
    public class TasksClient : ITasksClient
    {
        private readonly IAuthenticationClient _authenticationClient;
        public TasksClient(IAuthenticationClient c)
        {
            _authenticationClient = c;
        }
        public async Task<List<UserTaskDto>> GetSprintsTasksAsync(string token, string groupName)
        {
            var response = await _authenticationClient.SendRequestAsync(
                "tasks",
                $"GetSprintTasks/{groupName}",
                "GET",
                token,
                null
            );

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var tasks = JsonSerializer.Deserialize<List<UserTaskDto>>(response!, jsonOptions);

            return tasks!;
        }
    }
}
