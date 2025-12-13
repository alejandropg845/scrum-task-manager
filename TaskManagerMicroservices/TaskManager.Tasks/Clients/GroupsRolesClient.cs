using TaskManager.Common.Interfaces;
using TaskManager.Tasks.Interfaces;

namespace TaskManager.Tasks.Clients
{
    public class GroupsRolesClient : IGroupsRolesClient
    {
        private readonly IAuthenticationClient _authenticationClient;
        public GroupsRolesClient(IAuthenticationClient authClient)
        {
            _authenticationClient = authClient;
        }
        public async Task<string?> GetRoleNameAsync(string groupName, string username, string token)
        {
            var jsonResult = await _authenticationClient
                .SendRequestAsync(
                    "groups-roles",
                    $"GetUserGroupRole/{groupName}",
                    "get", 
                    token, 
                    null
                );

            string? role = jsonResult!.RootElement.GetProperty("result").GetString();

            return role;
        }
    }
}
