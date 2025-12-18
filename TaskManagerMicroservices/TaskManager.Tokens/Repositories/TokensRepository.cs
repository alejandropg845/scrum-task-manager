using MongoDB.Driver;
using System.Security.Cryptography;
using TaskManager.Common;
using TaskManager.Common.Documents;
using TaskManager.Common.Interfaces;
using TaskManager.Tokens.Interfaces;

namespace TaskManager.Tokens.Repositories
{
    public class TokensRepository : ITokensRepository
    {
        private readonly FilterDefinitionBuilder<Token> _filter = Builders<Token>.Filter;
        private readonly IMongoCollection<Token> _tokensCollection;
        public TokensRepository(IMongoCollection<Token> tokensCollection)
        {
            _tokensCollection = tokensCollection;
        }
        public async Task<string> CreateRefreshTokenAsync(string username, IClientSessionHandle? transaction)
        {
            var generatedToken = ExtendedConfigs.GenerateRefreshToken(username);

            if (transaction is not null)
                await _tokensCollection.InsertOneAsync(transaction, generatedToken);
            else
                await _tokensCollection.InsertOneAsync(generatedToken);

            return generatedToken.RefreshToken;

        }

        public async Task<Token> SaveRefreshTokenAsync(Token t)
        {
            await _tokensCollection.InsertOneAsync(t);
            return t;
        }
        
        public async Task<Token?> GetRefreshTokenByValueAsync(string? refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken) || refreshToken is "null" || refreshToken is "undefined") 
                return null;

            var filter = _filter.Eq(t => t.RefreshToken, refreshToken);

            var token = await _tokensCollection.Find(filter).FirstOrDefaultAsync();

            return token;
        }

        //public async Task DeleteOldRefreshTokenAsync(string id, IClientSessionHandle transaction)
        //{
        //    var filter = _filter.Eq(t => t.Id, id);

        //    await _tokensCollection.DeleteOneAsync(transaction, filter);

        //}
    }
}
