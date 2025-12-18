namespace TaskManager.Groups.Interfaces
{
    public interface IGroupReadRepository
    {
        Task<bool> GroupExistsAsync(string groupName);
        Task<bool> IsGroupOwnerAsync(string username, string groupName);
        Task<bool> IsScrumAsync(string groupName);
        Task<bool> IsAddingTasksAllowedAsync(string groupName);
        Task<(bool IsScrum, bool IsAddingTasksAllowed, bool IsGroupOwner)> GetInitialGroupInfoAsync(string groupName, string username);

    }
}
