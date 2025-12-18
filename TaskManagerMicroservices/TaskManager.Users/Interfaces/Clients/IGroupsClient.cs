using TaskManager.Common.DTOs;

namespace TaskManager.Users.Interfaces.Clients
{
    public interface IGroupsClient
    {
        Task<bool> IsScrumAsync(string groupName, string token);
        Task<(bool IsScrum, bool IsAllowed, bool IsGroupOwner)> GetGroupFeatures(string groupName, string token);
        Task<(string? UserGroupRole, ToSprintDto CurrentSprint, string? PreviousSprintId)> GetGroupRoleWithPreviousAndCurrentSprintAsync(string groupName, string token);
    }
}
