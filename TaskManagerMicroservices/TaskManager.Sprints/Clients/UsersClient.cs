using System.Text.Json;
using TaskManager.Common.DTOs;
using TaskManager.Common.Interfaces;
using TaskManager.Sprints.Interfaces;

namespace TaskManager.Sprints.Clients
{
    public class UsersClient : IUsersClient
    {
        private readonly IAuthenticationClient _authenticationClient;
        public UsersClient(IAuthenticationClient client)
        {
            _authenticationClient = client;
        }
        public async Task<IReadOnlyList<ToUserDto>> GetUsersAsync(string groupName, string token)
        {
            var jsonResponse = await _authenticationClient
                .SendRequestAsync(
                    "users",
                    $"GetUsers/{groupName}",
                    "get",
                    token,
                    null
                );

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            return jsonResponse!.Deserialize<IReadOnlyList<ToUserDto>>(options)!;
        }
    }
}
