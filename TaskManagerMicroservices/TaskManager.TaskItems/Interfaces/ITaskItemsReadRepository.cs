using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;

namespace TaskManager.TaskItems.Interfaces
{
    public interface ITaskItemsReadRepository
    {
        Task<IReadOnlyList<TaskItemDto>> GetTaskItemsAsync(string taskId);
        Task<bool> TaskItemExistsAsync(string taskItemId);
        Task<bool> IsAlreadyCompletedAsync(string taskItemId);
        Task<bool> IsAnyTaskItemNotCompleted(string taskId);
        Task<bool> MarkTaskItemAsCompleted(string taskItemId, string username);
        Task<IReadOnlyList<TaskItem>> GetUserPendingTaskItemsAsync(string username, string groupName);

    }
}
