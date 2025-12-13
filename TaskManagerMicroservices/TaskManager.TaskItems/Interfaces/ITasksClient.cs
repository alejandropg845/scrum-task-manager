namespace TaskManager.TaskItems.Interfaces
{
    public interface ITasksClient
    {
        Task<bool> TaskContainsSprintAsync(string taskId, string token);
        Task<bool> TaskExistsAsync(string taskId, string token);
        Task<bool> IsTaskOwnerAsync(string taskId, string token);
    }
}
