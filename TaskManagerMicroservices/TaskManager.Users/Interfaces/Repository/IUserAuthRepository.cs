using TaskManager.Common.Documents;

namespace TaskManager.Users.Interfaces.Repository
{
    public interface IUserAuthRepository
    {
        Task<(string userId, DateTimeOffset? RecoveryExpirationTime)> CheckReceivedRecoveryCodeAsync(string recoveryCode, string email);
        Task ChangeUserPasswordAsync(string password, string userId);
        string HashPassword(string password);
        Task SetRecoveryCodeAndExpirationTimeAsync(string email, string guid);
        Task<bool> IsGoogleAccountAsync(string username);
        Task<User?> GetGoogleAccount(string googleId);
        Task AddUserAsync(User user);
        Task<(string Name, string Id, bool IsError)> ValidateGoogleTokenAsync(string idToken, string clientId);
        bool VerifyPassword(string password, string storedHashedPassword);
        Task<bool> UserExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
    }
}
