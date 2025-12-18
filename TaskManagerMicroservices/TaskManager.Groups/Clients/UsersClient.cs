using Newtonsoft.Json.Linq;
using System.Text.Json;
using TaskManager.Common.Documents;
using TaskManager.Common.Interfaces;
using TaskManager.Groups.Interfaces;

namespace TaskManager.Groups.Clients
{
    public class UsersClient : IUsersClient
    {
        private readonly IAuthenticationClient _authenticationClient;
        public UsersClient(IAuthenticationClient c)
        {
            _authenticationClient = c;
        }

        public async Task<JsonDocument?> SetGroupToUserAsync(string groupName, string token, object? body)
        {
            return await _authenticationClient
            .SendRequestAsync(
                "users",
                $"SetGroupToUser/{groupName}",
                "post",
                token,
                body
            );
        }


        public Task DeleteGroupForMembersAsync(string groupName, string token)
        {
            var deleteGroupForMembers_Task = _authenticationClient
            .SendRequestAsync(
                "users", 
                $"RemoveGroupForMembers/{groupName}", 
                "put", 
                token, 
                null
            );

            return deleteGroupForMembers_Task;
        }
    }
}
