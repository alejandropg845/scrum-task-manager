using System.Text.Json;
using TaskManager.Common.Documents;
using TaskManager.Common.Interfaces;
using TaskManager.Groups.Interfaces;

namespace TaskManager.Groups.Clients
{
    public class GroupsRolesClient : IGroupsRolesClient
    {
        private readonly IAuthenticationClient _authenticationClient;
        public GroupsRolesClient(IAuthenticationClient c)
        {
            _authenticationClient = c;
        }
        public async Task<string?> GetUserGroupRoleAsync(string groupName, string token, object? body)
        {
            var getUserGroupRole_json = await _authenticationClient
            .SendRequestAsync(
                "groups-roles",
                $"GetUserGroupRole/{groupName}",
                "get",
                token,
                body
            );

            string? role = getUserGroupRole_json!.RootElement.GetProperty("result").ToString();

            return role;
        }

        public async Task SetUserGroupRoleAsync(string groupName, string username, string token, object? body)
        {
            await _authenticationClient
            .SendRequestAsync(
                "groups-roles",
                $"SetUserGroupRole/{username}?groupName={groupName}",
                "put",
                token,
                body
            );

        }

    }
}
