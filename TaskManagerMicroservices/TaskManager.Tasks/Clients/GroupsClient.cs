using Newtonsoft.Json.Linq;
using TaskManager.Common.Documents;
using TaskManager.Common.Interfaces;
using TaskManager.Tasks.Interfaces;

namespace TaskManager.Tasks.Clients
{
    public class GroupsClient : IGroupsClient
    {
        private readonly IAuthenticationClient _authenticationClient;
        public GroupsClient(IAuthenticationClient client)
        {
            _authenticationClient = client;
        }
        public async Task<bool> IsAddingTasksAllowed(string groupName, string token)
        {
            var jsonResult = await _authenticationClient
            .SendRequestAsync(
                "groups", 
                $"IsAddingTasksAllowed/{groupName}", 
                "get", 
                token, 
                null
            );

            return jsonResult!.RootElement.GetProperty("result").GetBoolean();
        }

        public async Task<bool> IsUserGroupOwnerAsync(string groupName, string username, string token)
        {
            var jsonResult = await _authenticationClient
                .SendRequestAsync(
                    "groups", 
                    $"UserGroupName/{groupName}",
                    "get",
                    token,
                    null
                );

            return jsonResult!.RootElement.GetProperty("result").GetBoolean();

        }
        public async Task<bool> IsScrumAsync(string groupName, string token)
        {
            var jsonResult = await _authenticationClient
                .SendRequestAsync(
                    "groups", 
                    $"IsScrum/{groupName}", 
                    "get", 
                    token, 
                    null
                );

            return jsonResult!.RootElement.GetProperty("result").GetBoolean();
        }
    }
}
