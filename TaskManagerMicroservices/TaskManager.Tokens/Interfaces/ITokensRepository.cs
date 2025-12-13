using MongoDB.Driver;
using TaskManager.Common.Documents;

namespace TaskManager.Tokens.Interfaces
{
    public interface ITokensRepository
    {
        Task<Token?> GetRefreshTokenByValueAsync(string? refreshToken);
        Task<string> CreateRefreshTokenAsync(string username, IClientSessionHandle? transaction);
        Task<Token> SaveRefreshTokenAsync(Token t);

    }
}
