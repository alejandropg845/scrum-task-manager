namespace TaskManager.Tasks.Interfaces
{
    public interface IGroupsClient
    {
        Task<bool> IsAddingTasksAllowed(string groupName, string token);
        Task<bool> IsUserGroupOwnerAsync(string groupName, string username, string token);
        Task<bool> IsScrumAsync(string groupName, string token);
    }
}
