using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Tasks.DTOs;

namespace TaskManager.Tasks.Interfaces
{
    public interface ITaskWriteService
    {
        Task<(UserTask? userTask, string ErrorMessage)> AddUserTaskAsync(string username, CreateTaskDto dto, string token);
        Task<DeleteTaskResponse> DeleteTaskAsync(string? groupName, string taskId, string token, string username);
        Task<List<string>> SetSprintToTasksAsync(SprintInfoForTask info);
        Task<bool> SetTaskAsCompletedAsync(string taskId);
        Task MarkSprintTasksAsFinishedAsync(string sprintId);
        Task<bool> SetTaskPriorityAsync(PrioritizeTaskDto dto, string username, string token);
        Task RevertSprintTasksSetAsFinishedAsync(string sprintId);

    }
}
