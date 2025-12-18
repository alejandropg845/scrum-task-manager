using TaskManager.Common.Interfaces;
using TaskManager.Sprints.Interfaces;

namespace TaskManager.Sprints.Clients
{
    public class GroupsRolesClient : IGroupsRolesClient
    {
        private readonly IAuthenticationClient _authenticationClient;
        public GroupsRolesClient(IAuthenticationClient auth)
        {
            _authenticationClient = auth;
        }
        public async Task<bool> IsAuthorizedByGroupRoleAsync(string groupName, string token)
        {
            var jsonResponse = await _authenticationClient
                .SendRequestAsync(
                    "groups-roles",
                    $"GetUserGroupRole/{groupName}",
                    "get",
                    token,
                    null
                );

            string? groupRole = jsonResponse!.RootElement.GetProperty("result").GetString();

            return groupRole is "product owner" || groupRole is "scrum master";
        }
    }
}
