
using TaskManager.Common;
using TaskManager.Common.Configurations;
using TaskManager.Common.Interfaces;
using TaskManager.Common.Services;
using TaskManager.Groups;
using TaskManager.Groups.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Logging.AddAzureWebAppDiagnostics();
builder.Services.AddAzureDiagnosticsConfig();

var mongoSettings = builder.Configuration
    .GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>()!;

var jwtSettings = builder.Configuration.GetSection(nameof(JWTSettings)).Get<JWTSettings>()!;

builder.Services.SetMongoConfiguration(mongoSettings).AddGroupsRepository(mongoSettings.CollectionName);

builder.Services.AddHttpClient(); // <== Agregar HttpClient para satisfacer  AuthenticationClient
builder.Services.CreateAuthenticationClient();

builder.Services.AddGroupsServiceRequiredParameters();

builder.Services.ConfigureCommonAuthentication(jwtSettings);
builder.Services.SetPollyPolicies<AuthenticationClient>();

//HOSTED SERVICES
builder.Services.AddHostedService<DisposeRabbitMq>();
builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();
builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
builder.Services.AddHostedService<GroupEventsListener>();

builder.Services.ConfigureAngularCors();

var app = builder.Build();

app.SetMiddlewares();
using var scope = app.Services.CreateScope();
await scope.ServiceProvider.SetGroupIndexesAsync(mongoSettings.CollectionName);

app.Run();
