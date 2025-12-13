using System.Text.Json;
using TaskManager.Common.Interfaces;
using TaskManager.Users.Interfaces.Clients;

namespace TaskManager.Users.Clients
{
    public class SprintsClient : ISprintsClient
    {
        private readonly IAuthenticationClient _authenticationClient;
        public SprintsClient(IAuthenticationClient c)
        {
            _authenticationClient = c;
        }
        public Task<JsonDocument?> CycleSprintAsync(string token, object? body)
        {
            return _authenticationClient.SendRequestAsync(
                "sprints",
                "sprints/CycleSprint",
                "put",
                token,
                body
            );
        }
    }
}
