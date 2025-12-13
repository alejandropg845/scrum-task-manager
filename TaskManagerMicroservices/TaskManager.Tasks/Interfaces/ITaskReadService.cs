using TaskManager.Common.DTOs;

namespace TaskManager.Tasks.Interfaces
{
    public interface ITaskReadService
    {
        Task<IReadOnlyList<UserTaskDto>> GetUserTasksAsync(string username, string groupName, string token);
        Task<string?> GetTaskOwnerNameAsync(string taskItemId);
        Task<List<UserTaskDto>> GetCompletedTasksAsync(string token, string groupName);
        Task<bool> TaskExistsAsync(string taskId);
        Task<bool> TaskContainsSprintAsync(string taskId);
        Task<bool> IsTaskOwnerAsync(string taskId, string username);

    }
}
