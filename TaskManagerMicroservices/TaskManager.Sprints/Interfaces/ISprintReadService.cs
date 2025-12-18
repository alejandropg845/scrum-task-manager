using TaskManager.Common.DTOs;

namespace TaskManager.Sprints.Interfaces
{
    public interface ISprintReadService
    {
        Task<List<ToSprintDto>> GetGroupSprintsAsync(string groupName);
        Task<bool> CanMarkSprintTaskItemAsCompletedAsync(string sprintId);
        Task<int> GetSprintNumberAsync(string groupName);

    }
}
