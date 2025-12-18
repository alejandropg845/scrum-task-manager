using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Common.Interfaces;

namespace TaskManager.Common
{
    public class CommonHub : Hub
    {
        private readonly IAuthenticationClient _authClient;
        public CommonHub(IAuthenticationClient a)
        {
            _authClient = a;
        }

        /* Obtención de Username */
        private string? GetUsername()
        {
            string token = Context.GetHttpContext()?.Request.Query["access_token"]!;
            var handler = new JwtSecurityTokenHandler();
            var username = handler.ReadJwtToken(token).Claims.First(c => c.Type == "unique_name").Value;

            return username;
        }

        #region Administración de conexiones
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }
        public override async Task<Task> OnDisconnectedAsync(Exception? exception)
        {
            string? username = GetUsername();


            if (username is not null)
            {
                string token = Context.GetHttpContext()?.Request.Query["access_token"]!;

                //Sacamos al usuario de su grupo en mongo
                var userGroup_jsonResponse = await _authClient
                    .SendRequestAsync("groups", "GetUserGroupName", "get", token, null);

                string? userGroupName = userGroup_jsonResponse?.RootElement
                    .GetProperty("result").GetString();

                if (userGroupName is not null)
                {
                    //Eliminamos del group del hub
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, userGroupName);
                }
            }

            return base.OnDisconnectedAsync(exception);

        }

        #endregion

        #region Administración de uniones en Groups
        public async Task OnJoinedGroup(string groupName, string roleName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            string? username = GetUsername();

            if (username is not null)
                await Clients.Group(groupName)
                    .SendAsync("onReceiveUserJoinedGroup", username, roleName);
        }

        public async Task OnLeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            string? username = GetUsername();

            if (username is not null) await OnSendUserLeftGroup(username, groupName);
        }

        public async Task OnRemoveGroup(string groupName)
        => await Clients.Group(groupName).SendAsync("onReceiveRemovedGroup", null);
        public async Task OnDeletedGroupTask(string taskId, string groupName)
        => await Clients.Group(groupName).SendAsync("onReceiveDeletedGroupTask", taskId);
        public async Task OnSendUserLeftGroup(string username, string groupName)
        => await Clients.Group(groupName).SendAsync("onReceiveUserLeftGroup", username);
        public async Task OnSendUserJoinedGroup(string username, string groupName)
        => await Clients.Group(groupName).SendAsync("onReceiveUserJoinedGroup", username);

        #endregion

        #region Administración de Tasks
        public async Task SendTaskToEveryone(UserTask task)
        => await Clients.Group(task.GroupName).SendAsync("onReceiveGroupTask", task);
        public async Task OnSendCompletedTask(string taskId, string groupName)
        => await Clients.Group(groupName).SendAsync("onReceiveCompletedTask", taskId);
        public async Task OnSendTaskPriority(string taskId, int priority, string groupName)
        => await Clients.Group(groupName).SendAsync("onReceiveTaskPriority", taskId, priority);

        #endregion

        #region Administración de TaskItems
        public async Task OnSendTaskItemToGroup(TaskItemDto taskItem)
        => await Clients.Group(taskItem.GroupName).SendAsync("onReceiveGroupTaskItem", taskItem);
        public async Task OnSendRemovedTaskItem(TaskItemDto taskItem)
        => await Clients.Group(taskItem.GroupName).SendAsync("onReceiveRemovedTaskItem", taskItem);
        public async Task OnSendCompletedTaskItem(TaskItemDto taskItem, string taskOwnerName, string username, bool taskIsCompleted)
        => await Clients.Group(taskItem.GroupName).SendAsync("OnReceiveCompletedTaskItem", taskItem, taskOwnerName, username, taskIsCompleted);
        public async Task OnSetTaskItemPriority(string taskItemId, string taskId, int priority, string groupName)
        => await Clients.Group(groupName).SendAsync("onReceiveTaskItemPriority", taskItemId, taskId, priority);
        public async Task OnSendUpdatedTaskItem(string taskId, string taskItemId, string newContent, string assignTo, string groupName)
        => await Clients.Group(groupName).SendAsync("onReceiveUpdatedTaskItem", taskId, taskItemId, newContent, assignTo);
        
        #endregion

        #region Administración de GroupsRoles
        public async Task OnUserGroupRoleSet(SetUserGroupRoleHubDto dto)
        => await Clients.Group(dto.GroupName).SendAsync(
            "onReceiveUserGroupRole", 
            dto.Username, 
            dto.GroupRole, 
            dto.UserThatAssignedProductOwner,
            dto.IsSwitchingScrumMaster,
            dto.UserThatWasScrumMaster,
            dto.UserThatIsScrumMaster
        );

        #endregion

        #region Administración de Sprints
        public async Task OnSetSprintToTasks(List<string> tasksIds, string groupName, DateTimeOffset expirationTime, string sprintId, string? sprintName, TimeSpan remainingTime)
        => await Clients.Group(groupName).SendAsync("onReceiveSprintTasks", tasksIds, expirationTime, sprintId, sprintName, remainingTime);

        #endregion

        #region Administración de Chat
        public async Task OnSendGroupMessage(string groupName, MessagesDate? date, Message? message)
        => await Clients.Group(groupName).SendAsync("onReceiveGroupMessage", date, message);

        #endregion

        #region Administración de Retrospectives
        public async Task OnSendRetro(ToRetrospectiveDto retro, string groupName)
        => await Clients.Group(groupName).SendAsync("onReceiveRetro", retro);

        #endregion
    }
}
