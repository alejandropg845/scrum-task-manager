using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Tasks.DTOs;

namespace TaskManager.Tasks.Interfaces
{
    public interface ITaskWriteRepository
    {
        Task<List<string>>SetSprintToTasksAsync(SprintInfoForTask info);
        Task<UserTask> DeleteTaskWithoutScrumAsync(string taskId);
        Task<UserTask> AddTaskAsync(UserTask task);
        Task<bool> SetTaskAsCompletedAsync(string taskId);
        Task MarkSprintTasksAsFinishedAsync(string sprintId);
        Task RevertInProgressStatusAsync(string sprintId);
        Task RevertSprintTasksSetAsFinishedAsync(string sprintId);
        Task<bool> SetTaskPriorityAsync(string taskId, string username, int priority);
    }
}