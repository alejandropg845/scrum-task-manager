using MongoDB.Driver;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;

namespace TaskManager.Sprints.Interfaces
{
    public interface IRetrospectivesRepository
    {
        Task<List<ToRetrospectiveDto>> GetRetrospectivesAsync(string groupName);
        Task<ToRetrospectiveDto> AddSprintRetroAsync(CreateRetrospectiveDto dto, IClientSessionHandle? transaction);
    }
}
