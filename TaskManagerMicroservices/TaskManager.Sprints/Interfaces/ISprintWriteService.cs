using MongoDB.Driver;
using TaskManager.Common.DTOs;
using TaskManager.Sprints.Responses;

namespace TaskManager.Sprints.Interfaces
{
    public interface ISprintWriteService
    {
        Task<ToSprintDto> CreateSprintAsync(string sprintId, string groupName, int sprintNumber);
        Task<(ToSprintDto CurrentSprint, string? PreviousSprint)> GetPreviousAndCurrentSprintAsync(string groupName);
        Task<BeginSprintResponse> BeginSprintAsync(BeginSprintDto dto);
        Task<(byte[] PdfBytes, bool IsAuthorized)> GenerateSummaryAsync(string token, string groupName);
        Task DeleteSprintAsync(string sprintId);
        Task<(string completedSprintId, string createdSprintId)> CycleSprintAsync(SprintToComplete s, string token);
        Task RevertCycledSprintAsync(string groupName, string completedSprintId, string createdSprintId);
    }
}
