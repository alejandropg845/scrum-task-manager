using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;

namespace TaskManager.Tasks.Interfaces
{
    public interface ITaskReadRepository
    {
        Task<IReadOnlyList<UserTask>> GetGroupTasksAsync(string groupName);
        Task<IReadOnlyList<UserTask>> GetUserTasksAsync(string username);
        Task<string?> GetTaskOwnerNameAsync(string taskItemId);
        Task<List<UserTaskDto>> GetCompletedTasksAsync(string groupName);
        Task<bool> TaskExistsAsync(string taskId);
        Task<bool> IsTaskOwnerAsync(string taskId, string username);
        Task<bool> TaskContainsSprintAsync(string taskId);

    }
}
