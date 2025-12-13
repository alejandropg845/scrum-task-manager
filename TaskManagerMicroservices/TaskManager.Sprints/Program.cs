using TaskManager.Common;
using TaskManager.Common.Configurations;
using TaskManager.Common.Interfaces;
using TaskManager.Common.Services;
using TaskManager.Sprints;
using TaskManager.Sprints.EventListeners;
using TaskManager.Sprints.Interfaces;
using TaskManager.Sprints.Repositories;
using TaskManager.Sprints.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.ConfigureAngularCors();

var mongoSettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>()!;
var feedbacksCollectionName = builder.Configuration[$"{nameof(MongoDbSettings)}:FeedbackCollectionName"]!;
var retrosCollectionName = builder.Configuration[$"{nameof(MongoDbSettings)}:RetrosCollectionName"]!;

builder.Services.SetMongoConfiguration(mongoSettings).AddSprintsRepository(mongoSettings);

builder.Services.AddRetrosRepository(retrosCollectionName);

builder.Services.AddFeedbackRepository(feedbacksCollectionName);
builder.Services.AddSingleton<IFeedbackService, FeedbackService>();
builder.Services.AddSprintsServiceRequiredAbstractions();

builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();
builder.Services.SetPollyPolicies<AuthenticationClient>();

builder.Services.AddHostedService<SprintEventListener>();

JWTSettings s = builder.Configuration.GetSection(nameof(JWTSettings)).Get<JWTSettings>()!;

builder.Logging.AddAzureWebAppDiagnostics();
builder.Services.AddAzureDiagnosticsConfig();

builder.Services.AddHttpClient();
builder.Services.CreateAuthenticationClient();

Microsoft.Playwright.Program.Main(["install"]);

builder.Services.ConfigureCommonAuthentication(s);

var app = builder.Build();

app.SetMiddlewares();

using var scope = app.Services.CreateScope();

await scope.ServiceProvider.SetSprintIndexesAsync(
    mongoSettings.CollectionName,
    feedbacksCollectionName,
    retrosCollectionName
);

app.Run();
