using DnsClient.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Polly;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TaskManager.Common.Configurations;
using TaskManager.Common.Interfaces;

namespace TaskManager.Common.Services
{
    public class AuthenticationClient : IAuthenticationClient
    {
        private readonly HttpClient _httpClient;
        private readonly IAsyncPolicy<HttpResponseMessage> _policy;
        private readonly string _base;
        private readonly string _signingKey;
        public AuthenticationClient(HttpClient client, IAsyncPolicy<HttpResponseMessage> policy, IConfiguration config)
        {
            _httpClient = client;
            _policy = policy;
            _base = config["Clients:AuthenticationClient"]!;
            _signingKey = config[$"{nameof(JWTSettings)}:SigningKey"]!;
        }
        public async Task<JsonDocument?> SendRequestAsync(string microservice, string endpoint, string method, string token, object? body)
        {
            string newToken = RecreateToken(token);

            if (endpoint.Contains("SaveRefreshToken"))
            {
                Console.WriteLine();
            }

            var response = await _policy.ExecuteAsync(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, _base);
                request.Headers.Add(nameof(method), method);
                request.Headers.Add(nameof(microservice), microservice);
                request.Headers.Add(nameof(endpoint), endpoint);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", newToken);

                request.Content = method is not "delete" ? JsonContent.Create(body) : null;

                return await _httpClient.SendAsync(request);

            });

            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            return string.IsNullOrEmpty(json) ? null : await response.Content.ReadFromJsonAsync<JsonDocument>();

        }

        private string RecreateToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(token);

            string issuer = jwtSecurityToken.Claims.First(c => c.Type == "iss").Value;
            string username = jwtSecurityToken.Claims.First(c => c.Type == "unique_name").Value;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signingKey));

            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {

                Audience = "api-gateway-audience",
                Issuer = issuer,
                Expires = DateTime.UtcNow.AddMinutes(1),
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.UniqueName, username)
                }),
                SigningCredentials = signingCredentials
            };

            return new JsonWebTokenHandler().CreateToken(tokenDescriptor);

        }
    }
}
