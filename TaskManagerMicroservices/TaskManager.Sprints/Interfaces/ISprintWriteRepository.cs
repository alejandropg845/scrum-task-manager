using MongoDB.Driver;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Sprints.Responses;

namespace TaskManager.Sprints.Interfaces
{
    public interface ISprintWriteRepository
    {
        Task<ToSprintDto> CreateSprintAsync(string sprintId, string groupName, int sprintsNumber);
        Task<Sprint> BeginSprintAsync(BeginSprintDto dto, DateTimeOffset expirationTime);
        Task<int> DeleteSprintsAsync(string groupName);
        Task AddSprintAsync(Sprint sprint, IClientSessionHandle transaction);
        Task SetSprintAsCompletedAsync(string sprintId, IClientSessionHandle transaction);
        Task DeleteSprintAsync(string sprintId, IClientSessionHandle? transaction);
        Task RevertSprintStatusAsync(string sprintId, string groupName, string status, IClientSessionHandle? transaction);
    }
}
