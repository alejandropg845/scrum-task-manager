using TaskManager.Common.DTOs;

namespace TaskManager.Sprints.Interfaces
{
    public interface IUsersClient
    {
        Task<IReadOnlyList<ToUserDto>> GetUsersAsync(string groupName, string token);
    }
}
