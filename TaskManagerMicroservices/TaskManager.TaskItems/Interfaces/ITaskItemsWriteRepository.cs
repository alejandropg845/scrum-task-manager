
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;

namespace TaskManager.TaskItems.Interfaces
{
    public interface ITaskItemsWriteRepository
    {
        Task<TaskItem> AddTaskItemAsync(CreateTaskItemDto dto);
        Task DeleteTaskItemAsync(string taskItemId);
        Task UpdateTaskItemAsync(UpdateTaskItemDto dto);
        Task<SetPriorityResponse> SetPriorityToTaskItemAsync(SetPriorityToTaskItemDto dto);
        Task<string> DeleteTaskItemsAsync(string taskId);
    }
}