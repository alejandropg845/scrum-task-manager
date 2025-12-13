namespace TaskManager.TaskItems.Interfaces
{
    public interface IGroupsClient
    {
        Task<bool> IsScrumAsync(string groupName, string token);
    }
}
