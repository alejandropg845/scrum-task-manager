using MongoDB.Driver;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;

namespace TaskManager.Sprints.Interfaces
{
    public interface IRetrospectivesService
    {
        Task<List<ToRetrospectiveDto>> GetRetrospectivesAsync(string groupName);
        Task<(ToRetrospectiveDto? retro, bool isError)> AddRetroAndFeedbackAsync(CreateRetrospectiveDto dto, string username);
        Task<bool> IsAuthorizedByGroupRoleAsync(string groupName, string token);
    }
}
