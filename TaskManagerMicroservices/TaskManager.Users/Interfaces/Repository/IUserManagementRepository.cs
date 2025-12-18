using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Users.Responses;

namespace TaskManager.Users.Interfaces.Repository
{
    public interface IUserManagementRepository
    {
        Task<string> SetGroupToUserAsync(string username, string? groupName);
        Task RemoveGroupFromUsersAsync(string groupName);
        Task LeaveGroupAsync(string username, string groupName);
        Task<IReadOnlyList<ToUserDto>> GetOnlyUsersAsync(string groupName);
        Task DeleteSetGroupAsync(string username);
    }
}