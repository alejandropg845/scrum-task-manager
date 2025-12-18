namespace TaskManager.Users.Interfaces.Service
{
    public interface IUserManagementService
    {
        Task<string> SetGroupToUserAsync(string username, string? groupName);
        Task LeaveGroupAsync(string username, string groupName);
    }
}
