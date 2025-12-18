using TaskManager.Common.Documents;
using TaskManager.Common.Interfaces;
using TaskManager.TaskItems.Interfaces;

namespace TaskManager.TaskItems.Clients
{
    public class SprintsClient : ISprintsClient
    {
        private readonly IAuthenticationClient _authenticationClient;
        public SprintsClient(IAuthenticationClient client)
        {
            _authenticationClient = client;
        }

        public async Task<bool> CanMarkTaskItemAsCompletedAsync(string sprintId, string token)
        {

            var jsonResponse = await _authenticationClient
            .SendRequestAsync(
                "sprints", 
                $"sprints/CanMarkSprintTaskItemAsCompleted/{sprintId}", 
                "get", 
                token,
                null
            );

            return jsonResponse!.RootElement.GetProperty("result").GetBoolean();

        }
    }
}
