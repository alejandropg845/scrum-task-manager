using TaskManager.Chats;
using TaskManager.Common;
using TaskManager.Common.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var mongoSettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>()!;

builder.Services.AddMongoClient(mongoSettings).AddMessagesDateRepository();

var jwtSettings = builder.Configuration.GetSection(nameof(JWTSettings)).Get<JWTSettings>()!;

builder.Logging.AddAzureWebAppDiagnostics();
builder.Services.AddAzureDiagnosticsConfig();

builder.Services.AddMessagesDateRepository();
builder.Services.AddMessagesRepository();


builder.Services.AddDatesServiceRequiredDependencies();

builder.Services.ConfigureCommonAuthentication(jwtSettings);

builder.Services.ConfigureAngularCors();



var app = builder.Build();

using var scope = app.Services.CreateScope();
await scope.ServiceProvider.SetMessagesDateIndexesAsync();

app.SetMiddlewares();

app.Run();
