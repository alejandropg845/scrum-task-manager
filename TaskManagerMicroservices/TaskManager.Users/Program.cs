using MongoDB.Driver;
using Polly;
using TaskManager.Common;
using TaskManager.Common.Configurations;
using TaskManager.Common.Interfaces;
using TaskManager.Common.Services;
using TaskManager.Users;
using TaskManager.Users.Interfaces;
using TaskManager.Users.Repositories;
using TaskManager.Users.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

//SCOPES
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddTransient<IMailRecoveryPasswordService, MailRecoveryPasswordService>();
builder.Services.AddHttpClient();
builder.Services.CreateAuthenticationClient();
builder.Services.AddUsersService();

builder.Logging.AddAzureWebAppDiagnostics();
builder.Services.AddAzureDiagnosticsConfig();


var mongoSettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>()!;

builder.Services.SetMongoConfiguration(mongoSettings).AddUsersRepository(mongoSettings);

builder.Services.AddSignalR();
builder.Services.ConfigureAngularCors();



var jwtSettings = builder.Configuration.GetSection(nameof(JWTSettings)).Get<JWTSettings>()!;

builder.Services.ConfigureCommonAuthentication(jwtSettings);
builder.Services.SetPollyPolicies<AuthenticationClient>();

//HOSTED SERVICES
builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();
builder.Services.AddHostedService<UsersEventListener>();
builder.Services.AddHostedService<DisposeRabbitMq>();


var app = builder.Build();

app.SetMiddlewares();

app.MapHub<CommonHub>("/commonHub");

using var scope = app.Services.CreateScope();

await scope.ServiceProvider.SetUserIndexesAsync(mongoSettings.CollectionName);

app.Run();
