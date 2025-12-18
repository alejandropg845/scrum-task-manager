using TaskManager.Common.DTOs;

namespace TaskManager.Sprints.Interfaces
{
    public interface ISprintReadRepository
    {
        Task<int> GetSprintNumberAsync(string groupName);
        Task<ToSprintDto> GetCurrentSprintAsync(string groupName);
        Task<string?> GetPreviousSprintIdAsync(string groupName);
        Task<List<ToSprintDto>> GetGroupSprintsAsync(string groupName);
        Task<bool> CanMarkSprintTaskItemAsCompletedAsync(string sprintId);
        Task<List<ToSprintDto>> GetCompletedSprintsForSummaryAsync(string groupName);

    }
}
