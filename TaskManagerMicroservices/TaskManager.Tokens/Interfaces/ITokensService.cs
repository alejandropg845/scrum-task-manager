using TaskManager.Common.Documents;

namespace TaskManager.Tokens.Interfaces
{
    public interface ITokensService
    {
        Task<Token> SaveRefreshTokenAsync(Token t);
        Task<RefreshTokenResponse> GetNewAccessTokenAsync(string refreshToken);
    }
}
