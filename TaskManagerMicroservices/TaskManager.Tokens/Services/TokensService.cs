using MongoDB.Driver;
using System.Data.Common;
using System.Transactions;
using TaskManager.Common.Configurations;
using TaskManager.Common.Documents;
using TaskManager.Common.Interfaces;
using TaskManager.Tokens.Interfaces;

namespace TaskManager.Tokens.Services
{
    public class TokensService : ITokensService
    {
        private readonly ITokensRepository _repo;
        private readonly ITokenService _tokenService;
        private readonly IMongoClient _mongoClient;
        private readonly ILogger<TokensService> _logger;
        private readonly string _audience;
        private readonly string _issuer;
        public TokensService(ITokensRepository tokensRepository, ITokenService tokenService, IConfiguration config, IMongoClient mongoClient, ILogger<TokensService> logger)
        {
            _repo = tokensRepository;
            _tokenService = tokenService;
            _issuer = config[$"{nameof(JWTSettings)}:Issuer"]!;
            _audience = config[$"{nameof(JWTSettings)}:AudienceApiGateway"]!;
            _mongoClient = mongoClient;
            _logger = logger;
        }
        public async Task<RefreshTokenResponse> GetNewAccessTokenAsync(string refreshToken)
        {
            var response = new RefreshTokenResponse();

            /* Token existe */
            var token = await _repo.GetRefreshTokenByValueAsync(refreshToken);

            response.IsAnyIssue = token is null;
            if (token is null) return response;
            

            /* Validar expiration */
            if (token.Expiration <= DateTimeOffset.UtcNow)
            {
                response.IsAnyIssue = true;
                return response;
            }

            string accessToken = _tokenService.GenerateToken(token.Username, _audience, _issuer);

            response.AccessToken = accessToken;
            response.RefreshToken = token.RefreshToken;

            return response;
        }
        public async Task<Token> SaveRefreshTokenAsync(Token t)
        => await _repo.SaveRefreshTokenAsync(t);

    }
}
