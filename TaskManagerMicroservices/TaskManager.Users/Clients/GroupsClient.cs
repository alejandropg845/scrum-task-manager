using Newtonsoft.Json.Linq;
using System.Text.Json;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Common.Interfaces;
using TaskManager.Users.Interfaces.Clients;

namespace TaskManager.Users.Clients
{
    public class GroupsClient : IGroupsClient
    {
        private readonly IAuthenticationClient _authenticationClient;
        public GroupsClient(IAuthenticationClient auth)
        {
            _authenticationClient = auth;
        }
        public async Task<bool> IsScrumAsync(string groupName, string token)
        {
            //var jsonResponse = await _authenticationClient.SendRequestAsync(
            //    "groups-roles", 
            //    $"GetUsersGroupRoles/{groupName}", 
            //    "get", 
            //    token, 
            //    null
            //);

            var jsonResponse = await _authenticationClient.SendRequestAsync(
               "groups",
               $"IsScrum/{groupName}",
               "get",
               token,
               null
            );

            return jsonResponse!.RootElement.GetProperty("result").GetBoolean();

        }
        public async Task<(bool IsScrum, bool IsAllowed, bool IsGroupOwner)> GetGroupFeatures(string groupName, string token)
        {
            var response = await _authenticationClient.SendRequestAsync(
                "groups", 
                $"GetInitialGroupInfo/{groupName}", 
                "get", 
                token, 
                null
            );

            bool isScrum = response!.RootElement.GetProperty("isScrum").GetBoolean();
            bool isAddingTasksAllowed = response.RootElement.GetProperty("isAddingTasksAllowed").GetBoolean();
            bool isGroupOwner = response.RootElement.GetProperty("isGroupOwner").GetBoolean();

            return new(isScrum, isAddingTasksAllowed, isGroupOwner);
        }

        public async Task<(string? UserGroupRole, ToSprintDto CurrentSprint, string? PreviousSprintId)> GetGroupRoleWithPreviousAndCurrentSprintAsync(string groupName, string token)
        {
            var getUserGroupRole_Task = _authenticationClient
                .SendRequestAsync(
                    "groups-roles", 
                    $"GetUserGroupRole/{groupName}",
                    "get", 
                    token, 
                    null
                );

            var getGroupSprint_Task = _authenticationClient
                .SendRequestAsync(
                    "sprints", 
                    $"sprints/GetSprint/{groupName}", 
                    "get", 
                    token, 
                    null
                );


            await Task.WhenAll(getUserGroupRole_Task, getGroupSprint_Task);

            string? userGroupRole = (await getUserGroupRole_Task)!
                .RootElement.GetProperty("result").GetString();

            var sprint_jsonTask = await getGroupSprint_Task;

            var currentSprint = sprint_jsonTask!.RootElement
                .GetProperty("currentSprint")!
                .Deserialize<ToSprintDto>()!;

            string? previousSprintId = sprint_jsonTask
                .RootElement
                .GetProperty("previousSprintId")
                .GetString();

            return new(userGroupRole, currentSprint, previousSprintId);
        }
    }
}

