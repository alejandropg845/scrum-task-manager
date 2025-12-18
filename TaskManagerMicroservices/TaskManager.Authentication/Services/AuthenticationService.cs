using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using TaskManager.Authentication.Interfaces;
using TaskManager.Common.Configurations;
using TaskManager.Common.DTOs;
using TaskManager.Common.Interfaces;

namespace TaskManager.Authentication.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly Microservices _microservices;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly string _issuer;
        private readonly ITokenService _tokenService;
        public AuthenticationService(IConfiguration configuration, HttpClient httpClient, ITokenService tokenService, ILogger<AuthenticationService> logger)
        {
            _microservices = configuration.GetSection(nameof(Microservices)).Get<Microservices>()!;
            _httpClient = httpClient;
            _logger = logger;
            _issuer = configuration[$"{nameof(JWTSettings)}:Issuer"]!;
            _tokenService = tokenService;
        }
        public async Task<HttpResponseMessage> SendRequestAsync(RequestInfo r)
        {
            string responseBody = "";


            if (r.Endpoint.Contains("CycleSprint"))
            {
                Console.WriteLine();
            }


            try
            {
                HttpMethod httpMethod = SetHttpRequestMethod(r.Method);
                string url = GetRequestRoute(r.Microservice, r.Endpoint);

                string audience = $"{r.Microservice}-microservice-audience";

                /* Create JWT */
                string token = _tokenService.GenerateToken(r.Username, audience, _issuer);

                var request = new HttpRequestMessage(httpMethod, url);

                request.Content = JsonContent.Create(r.JsonBody);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);

                var response = await _httpClient.SendAsync(request);

                responseBody = await response.Content.ReadAsStringAsync();

                return response;
            } catch (Exception ex)
            {
                _logger.LogError(
                    "Error en AuthenticationService\nInformación de la petición:\nMethod: {Method}\nMicroservice: {Microservice}\nEndpoint: {Endpoint}\nUsername: {Username}\nRespuesta del body: {body}" +
                    "Excepción: {Msg}\n"+
                    "StackTrace: {StackTrace}",
                    r.Method,
                    r.Microservice,
                    r.Endpoint,
                    r.Username,
                    responseBody ?? "none",
                    ex.Message, 
                    ex.StackTrace
                );
                throw;
            }

        }
        private static HttpMethod SetHttpRequestMethod(string method)
        {
            return method.ToUpper() switch
            {
                "GET" => HttpMethod.Get,
                "PUT" => HttpMethod.Put,
                "POST" => HttpMethod.Post,
                "DELETE" => HttpMethod.Delete,
                _ => throw new Exception("a non-existing HTTP method was provided"),
            };
        }
        
        private string GetRequestRoute(string microservice, string endpoint)
        {
            return microservice switch
            {
                "users" => _microservices.Users + endpoint,
                "sprints" => _microservices.Sprints + endpoint,
                "task-items" => _microservices.TaskItems + endpoint,
                "tasks" => _microservices.Tasks + endpoint,
                "groups" => _microservices.Groups + endpoint,
                "groups-roles" => _microservices.GroupsRoles + endpoint,
                "chats" => _microservices.Chats + endpoint,
                "tokens" => _microservices.Tokens + endpoint,
                _ => throw new Exception($"{microservice} is not a valid value")
            };

        }
    }
}
