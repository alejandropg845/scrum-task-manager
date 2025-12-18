using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Common.Configurations;
using Polly;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using TaskManager.Common.Interfaces;
using TaskManager.Common.Services;
using System.Globalization;
using System.Text.RegularExpressions;
using TaskManager.Common.Middlewares;
using Microsoft.Extensions.Logging.AzureAppServices;
using System.Security.Cryptography;
using TaskManager.Common.Documents;

namespace TaskManager.Common
{
    public static class ExtendedConfigs
    {
        public static IServiceCollection ConfigureCommonAuthentication(this IServiceCollection Services, JWTSettings jwtSettings)
        {
            Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o => {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey))
                };
            });

            return Services;

        }

        public static IServiceCollection ConfigureAngularCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("clientCORS", policy =>
                {
                    policy.SetIsOriginAllowed(origin =>
                    {
                        return origin.Contains(
                            "localhost",
                            StringComparison.OrdinalIgnoreCase
                        );
                    })
                    .AllowCredentials().AllowAnyHeader().AllowAnyMethod();
                });
            });

            return services;
        }

        public static IServiceCollection SetPollyPolicies<T>(this IServiceCollection services)
        {
            services.AddSingleton<IAsyncPolicy<HttpResponseMessage>>(serviceProvider =>
            {

                var logger = serviceProvider.GetRequiredService<ILogger<T>>();

                var retryPolicy =
                Policy.Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync
                (
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(3, attempt - 1)),
                    (outcome, timespan, attempt) =>
                    {
                        logger.LogWarning
                        ("Request failed. Trying with attempt {attempt}\n" +
                        "Exception was: {exception}",
                        attempt, outcome.Exception?.Message ?? "empty");
                    }
                );

                var timeoutPolicy =
                Policy.TimeoutAsync<HttpResponseMessage>
                (
                    TimeSpan.FromSeconds(20),
                    (context, timespan, task) =>
                    {
                        logger.LogWarning("Request timed out after {timespan} seconds", timespan.TotalSeconds);
                        return Task.CompletedTask;
                    }

                );

                return Policy.WrapAsync(retryPolicy, timeoutPolicy);

            });


            return services;
        }

        public static IServiceCollection CreateAuthenticationClient(this IServiceCollection services)
        {
            services.AddSingleton<IAuthenticationClient>(serviceProvider =>
            {
                var policy = serviceProvider.GetRequiredService<IAsyncPolicy<HttpResponseMessage>>();
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                var httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();

                return new AuthenticationClient(httpClient, policy, config);
            });


            return services;
        }

        public static Token GenerateRefreshToken(string username)
        {
            byte[] randomCharacters = new byte[32];
            using var rng = RandomNumberGenerator.Create();

            rng.GetBytes(randomCharacters);

            /* Eliminar caracteres sensibles para el header del apiGateway */
            string refreshToken = Convert.ToBase64String(randomCharacters)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');

            var newToken = new Token
            {
                Expiration = DateTimeOffset.UtcNow.AddMonths(1),
                Id = Guid.NewGuid().ToString(),
                RefreshToken = refreshToken,
                Username = username
            };

            return newToken;
        }
        public static string RemoveDiacritics(this string str)
        {
            string normalized = str.Normalize(NormalizationForm.FormD);
            StringBuilder builder = new StringBuilder();

            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(c);
                }
            }

            string normalizedString = builder.ToString();

            return RemoveSpecialCharacters(normalizedString);

        }
        private static string RemoveSpecialCharacters(string str)
        => Regex.Replace(str, "[^a-zA-Z0-9 ]+", "", RegexOptions.Compiled);

        public static WebApplication SetMiddlewares(this WebApplication app)
        {
            app.UseMiddleware<ExceptionHandling>();
            app.UseHttpsRedirection(); // <== Comentado para usar http en las peticiones de Docker
            app.UseRouting();
            app.UseCors("clientCORS");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            return app;
        }

        public static IServiceCollection AddAzureDiagnosticsConfig(this IServiceCollection services)
        {
            services.Configure<AzureFileLoggerOptions>(options =>
            {
                options.FileName = "logs-";
                options.FileSizeLimit = 50 * 1024;
                options.RetainedFileCountLimit = 5;
            });

            return services;
        }

    }

}
