using TaskManager.Common.Configurations;
using TaskManager.Common;
using TaskManager.GroupsRoles;
using TaskManager.GroupsRoles.Services;
using TaskManager.Common.Interfaces;
using TaskManager.Common.Services;
using TaskManager.GroupsRoles.Interfaces;
using TaskManager.GroupsRoles.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var jwtSettings = builder.Configuration.GetSection(nameof(JWTSettings)).Get<JWTSettings>()!;

builder.Services.ConfigureCommonAuthentication(jwtSettings);

builder.Services.ConfigureAngularCors();

builder.Logging.AddAzureWebAppDiagnostics();
builder.Services.AddAzureDiagnosticsConfig();

var mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>()!;

builder.Services.SetMongoConfiguration(mongoDbSettings).SetRepository(mongoDbSettings.CollectionName);

builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
builder.Services.AddHostedService<GroupRolesEventListener>();
builder.Services.AddSingleton<IGroupsRolesService, GroupsRolesService>();

builder.Services.SetPollyPolicies<AuthenticationClient>();

var app = builder.Build();

using var scope = app.Services.CreateScope();
await scope.ServiceProvider.SetGroupRoleIndexesAsync(mongoDbSettings.CollectionName);
app.SetMiddlewares();

app.Run();
