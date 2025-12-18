using TaskManager.Common.Documents;
using TaskManager.Common.Interfaces;
using TaskManager.TaskItems.Interfaces;

namespace TaskManager.TaskItems.Clients
{
    public class GroupsRolesClient : IGroupsRolesClient
    {
        private readonly IAuthenticationClient _authenticationClient;
        public GroupsRolesClient(IAuthenticationClient client)
        {
            _authenticationClient = client;
        }
        public async Task<string?> GetUserGroupRoleAsync(string groupName, string token)
        {
            var isProductOwner_response = await _authenticationClient
                .SendRequestAsync(
                    "groups-roles", 
                    $"GetUserGroupRole/{groupName}", 
                    "get", 
                    token, 
                    null
                );

            string? roleName = isProductOwner_response!.RootElement.GetProperty("result").GetString();

            return roleName;
        }
    }
}
