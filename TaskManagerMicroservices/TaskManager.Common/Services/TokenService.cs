
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Common.Configurations;
using TaskManager.Common.Interfaces;

namespace TaskManager.Common.Services
{
    public class TokenService : ITokenService
    {
        private readonly string _signingKey;
        public TokenService(IConfiguration config)
        {
            _signingKey = config[$"{nameof(JWTSettings)}:SigningKey"]!;
        }
        public string GenerateToken(string username, string audience, string issuer)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signingKey));

            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claim = new Claim(JwtRegisteredClaimNames.UniqueName, username);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity([claim]),
                Audience = audience,
                Issuer = issuer,
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = signingCredentials
            };

            return new JsonWebTokenHandler().CreateToken(tokenDescriptor);

        }
    }
}