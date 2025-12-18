using TaskManager.Common.Interfaces;
using TaskManager.TaskItems.Interfaces;

namespace TaskManager.TaskItems.Clients
{
    public class GroupsClient : IGroupsClient
    {
        private readonly IAuthenticationClient _authenticationClient;
        public GroupsClient(IAuthenticationClient client)
        {
            _authenticationClient = client;
        }
        public async Task<bool> IsScrumAsync(string groupName, string token)
        {
            var isScrumJsonResponse = await _authenticationClient
            .SendRequestAsync(
                "groups", 
                $"IsScrum/{groupName}", 
                "get", 
                token,
                null
            );

            bool isScrum = isScrumJsonResponse!.RootElement.GetProperty("result").GetBoolean()!;

            return isScrum;
        }
    }
}
