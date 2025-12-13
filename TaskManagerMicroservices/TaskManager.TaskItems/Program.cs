using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Common;
using TaskManager.Common.Configurations;
using TaskManager.Common.Interfaces;
using TaskManager.Common.Services;
using TaskManager.TaskItems;
using TaskManager.TaskItems.Clients;
using TaskManager.TaskItems.Interfaces;
using TaskManager.TaskItems.Repositories;
using TaskManager.TaskItems.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();

builder.Services.CreateAuthenticationClient();
builder.Services.AddHttpClient();

builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();
builder.Services.AddSingleton<IGeminiClient, GeminiClient>();
builder.Services.AddTaskItemsService();

var mongoSettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>()!;

// SINGLETONS 
builder.Services.SetMongoConfiguration(mongoSettings)
    .AddTaskItemsRepository(mongoSettings.CollectionName);

builder.Services.ConfigureAngularCors();

builder.Logging.AddAzureWebAppDiagnostics();
builder.Services.AddAzureDiagnosticsConfig();

var jwtSettings = builder.Configuration.GetSection(nameof(JWTSettings)).Get<JWTSettings>()!;

builder.Services.ConfigureCommonAuthentication(jwtSettings);



builder.Services.AddHostedService<TaskItemsEventListener>();

builder.Services.SetPollyPolicies<AuthenticationClient>();

var app = builder.Build();

app.SetMiddlewares();
using var scope = app.Services.CreateScope();
await scope.ServiceProvider.SetTaskItemIndexesAsync(mongoSettings.CollectionName);

app.Run();
