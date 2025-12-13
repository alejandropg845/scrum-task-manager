using System.Text.Json;
using TaskManager.Groups.Responses;

namespace TaskManager.Groups.Interfaces
{
    public interface IGroupWriteService
    {
        Task<(bool GroupExists, string? GroupName, string? GroupRoleName)> JoinGroupAsync(string username, string groupname, string token);
        Task<CreateGroupResponse> CreateGroupAsync(string username, string groupname, bool isScrum, string token);
        Task<bool> SetAddingTasksAllowed(string username, string groupName, bool isAllowed);
        Task<JsonDocument?> SetSprintIdToGroupAsync(string groupName, string sprintId);
        Task DeleteCreatedGroupAsync(string groupName);
        Task<RemoveGroupResponse> RemoveGroupAsync(string username, string groupName);
        
    }
}
