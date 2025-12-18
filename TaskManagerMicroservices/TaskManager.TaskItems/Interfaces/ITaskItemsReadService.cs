using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;

namespace TaskManager.TaskItems.Interfaces
{
    public interface ITaskItemsReadService
    {
        Task<IReadOnlyList<TaskItemDto>> GetTaskItemsAsync(string taskId);
        Task<IReadOnlyList<TaskItem>> GetUserPendingTaskItemsAsync(string username, string groupName);

    }
}
