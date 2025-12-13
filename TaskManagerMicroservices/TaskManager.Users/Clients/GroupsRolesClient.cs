using System.Text.Json;
using TaskManager.Common.Interfaces;
using TaskManager.Users.Interfaces.Clients;

namespace TaskManager.Users.Clients
{
    public class GroupsRolesClient : IGroupsRolesClient
    {
        private readonly IAuthenticationClient _authenticationClient;
        public GroupsRolesClient(IAuthenticationClient c)
        {
            _authenticationClient = c;
        }
        public Task<JsonDocument?> GetUsersGroupRolesAsync(string groupName, string token)
        { 
            return _authenticationClient
            .SendRequestAsync(
                "groups-roles", 
                $"GetUsersGroupRoles/{groupName}", 
                "get", 
                token, 
                null
            );
        }
    }
}
