using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;

namespace TaskManager.Users.Interfaces.Repository
{
    public interface IUserInfoRepository
    {
        Task<string?> GetUserGroupNameAsync(string username);
        Task<User?> GetUserAsync(string username);
        Task<(string? GroupName, string AvatarBgColor)> GetGroupNameAndAvatarBgColorAsync(string username);
    }
}
