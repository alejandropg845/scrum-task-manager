using Newtonsoft.Json.Linq;
using System.Text.Json;
using TaskManager.Common.Documents;
using TaskManager.Common.Interfaces;
using TaskManager.Groups.Interfaces;

namespace TaskManager.Groups.Clients
{
    public class SprintsClient : ISprintsClient
    {
        private readonly IAuthenticationClient _authenticationClient;
        public SprintsClient(IAuthenticationClient c)
        {
            _authenticationClient = c;
        }
        public async Task<JsonDocument> CreateSprintAsync(string sprintId, string groupName, string token, object? body)
        {
            var jsonResponse = await _authenticationClient
            .SendRequestAsync
            (
                "sprints",
                $"sprints/CreateSprint/{groupName}?sprintNumber=1&sprintId={sprintId}", 
                "post", 
                token, 
                body
            );

            return jsonResponse!;
        }
    }
}
