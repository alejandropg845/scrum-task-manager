using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Users.Responses;

namespace TaskManager.Users.Interfaces.Service
{
    public interface IUserAuthService
    {
        Task<IReadOnlyList<ToUserDto>> GetUsersAsync(string groupName, string token);
        Task<LoginUserResponse> LoginUserAsync(LoginUserDto dto);
        Task<RegisterUserResponse> RegisterUserAsync(RegisterUserDto dto);
        Task<ContinueWithGoogleResponse> ContinueWithGoogleAsync(string tokenId);
        Task RecoverPasswordAsync(string email);
        Task<ReceiveRecoveryCodeResponse> ReceiveRecoveryCodeAsync(string recoveryCode, string email, string password1, string password2);
    }
}
