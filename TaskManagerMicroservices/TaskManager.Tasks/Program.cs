using Polly;
using TaskManager.Common;
using TaskManager.Common.Configurations;
using TaskManager.Common.Interfaces;
using TaskManager.Common.Services;
using TaskManager.Tasks;
using TaskManager.Tasks.Interfaces;
using TaskManager.Tasks.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.ConfigureAngularCors();

var settings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>()!;

builder.Services.SetMongoConfiguration(settings).AddTasksRepository(settings.CollectionName);

var jwtSettings = builder.Configuration.GetSection(nameof(JWTSettings)).Get<JWTSettings>()!;

builder.Services.ConfigureCommonAuthentication(jwtSettings);
builder.Services.CreateAuthenticationClient();

builder.Logging.AddAzureWebAppDiagnostics();
builder.Services.AddAzureDiagnosticsConfig();

builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();
builder.Services.AddTasksService();
builder.Services.AddHostedService<TaskEventListener>();
builder.Services.SetPollyPolicies<AuthenticationClient>();

builder.Services.AddHttpClient();

var app = builder.Build();

app.SetMiddlewares();

using var scope = app.Services.CreateScope();
await scope.ServiceProvider.SetTaskIndexesAsync(settings.CollectionName);

app.Run();
