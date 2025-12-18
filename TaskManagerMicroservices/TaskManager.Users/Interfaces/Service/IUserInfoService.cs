using TaskManager.Common.DTOs;

namespace TaskManager.Users.Interfaces.Service
{
    public interface IUserInfoService
    {
        Task<InitialInfoDto> GetUserInfoAsync(string username, string token);
        Task<string?> GetUserGroupNameAsync(string username);

    }
}
