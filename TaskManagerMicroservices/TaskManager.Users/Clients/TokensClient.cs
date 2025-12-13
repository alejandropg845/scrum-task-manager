using TaskManager.Common.Documents;
using TaskManager.Common.Interfaces;
using TaskManager.Users.Interfaces.Clients;
using static Google.Apis.Requests.BatchRequest;

namespace TaskManager.Users.Clients
{
    public class TokensClient : ITokensClient
    {
        private readonly IAuthenticationClient _authenticationClient;
        public TokensClient(IAuthenticationClient client)
        {
            _authenticationClient = client;
        }
        public Task SaveRefreshTokenAsync(string token, Token t)
        {
            return _authenticationClient
            .SendRequestAsync(
                "tokens",
                $"SaveRefreshToken",
                "post",
                token,
                t
            );
        }
    }
}
