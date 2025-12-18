using TaskManager.Common.Documents;

namespace TaskManager.Users.Interfaces.Clients
{
    public interface ITokensClient
    {
        Task SaveRefreshTokenAsync(string token, Token t);
    }
}
