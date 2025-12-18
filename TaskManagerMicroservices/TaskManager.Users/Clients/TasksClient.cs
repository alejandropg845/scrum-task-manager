using System.Text.Json;
using TaskManager.Common.Documents;
using TaskManager.Common.Interfaces;
using TaskManager.Users.Interfaces.Clients;

namespace TaskManager.Users.Clients
{
    public class TasksClient : ITasksClient
    {
        private readonly IAuthenticationClient _authenticationClient;

        public TasksClient(IAuthenticationClient a)
        {
            _authenticationClient = a;
        }

        public Task<JsonDocument?> MarkSprintTasksAsFinishedAsync(string token, string sprintId)
        {

            return _authenticationClient.SendRequestAsync(
            "tasks",
                $"MarkSprintTasksAsFinished/{sprintId}",
                "put",
                token,
                null
            );
        }
    }
}
