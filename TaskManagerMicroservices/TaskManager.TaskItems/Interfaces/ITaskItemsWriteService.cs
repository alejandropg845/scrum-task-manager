using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;

namespace TaskManager.TaskItems.Interfaces
{
    public interface ITaskItemsWriteService
    {
        Task<(TaskItem ti, bool AssignToUserError, bool ContainsScrum)> CreateTaskItemAsync(CreateTaskItemDto dto, string username, string token);
        Task<MarkTaskItemAsCompletedResponse> SetTaskItemAsCompletedAsync(string username, MarkTaskItemAsCompletedDto dto, string token);
        Task<bool> DeleteTaskItemAsync(DeleteTaskItemDto dto, string token);
        Task<SetPriorityResponse> SetPriorityToTaskItemAsync(SetPriorityToTaskItemDto dto, string username, string token);
        Task<string> DeleteTaskItemsAsync(string taskId);
        Task<string> AskToGeminiAsync(AskToAssistantDto dto);
        Task<UpdateTaskItemResponse> UpdateTaskItemAsync(UpdateTaskItemDto dto, string token);

    }
}
