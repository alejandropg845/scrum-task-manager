using TaskManager.Authentication;
using TaskManager.Authentication.Controllers;
using TaskManager.Authentication.Interfaces;
using TaskManager.Authentication.Services;
using TaskManager.Common;
using TaskManager.Common.Configurations;
using TaskManager.Common.Interfaces;
using TaskManager.Common.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddHttpClient<AuthenticationService>();

builder.Services.ConfigureAngularCors();

var jwtSettings = builder.Configuration.GetSection(nameof(JWTSettings)).Get<JWTSettings>()!;

builder.Services.ConfigureAuthentication(jwtSettings.Audience, jwtSettings.SigningKey, jwtSettings.Issuer);

builder.Logging.AddAzureWebAppDiagnostics();
builder.Services.AddAzureDiagnosticsConfig();

builder.Services.AddControllers();

var app = builder.Build();

app.SetMiddlewares();

app.Run();
