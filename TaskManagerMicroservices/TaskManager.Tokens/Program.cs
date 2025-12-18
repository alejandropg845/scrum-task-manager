using TaskManager.Common.Configurations;
using TaskManager.Tokens;
using TaskManager.Common;
using TaskManager.Tokens.Interfaces;
using TaskManager.Tokens.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var mongoSettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>()!;

builder.Services.SetMongoClient(mongoSettings);
builder.Services.AddTokensRepository(mongoSettings.CollectionName);

builder.Services.AddTokensService();

var jwtSettings = builder.Configuration.GetSection(nameof(JWTSettings)).Get<JWTSettings>()!;

builder.Services.ConfigureCommonAuthentication(jwtSettings);

builder.Logging.AddAzureWebAppDiagnostics();
builder.Services.AddAzureDiagnosticsConfig();

builder.Services.ConfigureAngularCors();

var app = builder.Build();

app.SetMiddlewares();

using var scope = app.Services.CreateScope();

await scope.ServiceProvider.SetTokensIndexesAsync(mongoSettings.CollectionName);

app.Run();
