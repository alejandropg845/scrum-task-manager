using MongoDB.Bson;
using MongoDB.Driver;
using TaskManager.Common.Configurations;
using TaskManager.Common.Documents;
using TaskManager.Common.Interfaces;
using TaskManager.Users.Clients;
using TaskManager.Users.Interfaces;
using TaskManager.Users.Interfaces.Clients;
using TaskManager.Users.Interfaces.Repository;
using TaskManager.Users.Interfaces.Service;
using TaskManager.Users.Repositories;
using TaskManager.Users.Services;

namespace TaskManager.Users
{
    public static class Extensions
    {
        public static IServiceCollection SetMongoConfiguration(this IServiceCollection services, MongoDbSettings settings)
        {
            var mongoClient = new MongoClient(settings.ConnectionString);
            services.AddSingleton<IMongoClient>(mongoClient);

            services.AddSingleton<IMongoDatabase>(serviceProvider =>
            {
                return mongoClient.GetDatabase(settings.DatabaseName);
            });

            return services;
        }
        public static IServiceCollection AddUsersRepository(this IServiceCollection services, MongoDbSettings settings)
        {
            services.AddSingleton(serviceProvider =>
            {
                var collection = serviceProvider.GetRequiredService<IMongoDatabase>()
                .GetCollection<User>(settings.CollectionName);
                return new UsersRepository(collection);

            });

            services.AddSingleton<IUserAuthRepository>(sp => sp.GetRequiredService<UsersRepository>());
            services.AddSingleton<IUserInfoRepository>(sp => sp.GetRequiredService<UsersRepository>());
            services.AddSingleton<IUserManagementRepository>(sp => sp.GetRequiredService<UsersRepository>());


            return services;
        }
        public static IServiceCollection AddUsersService(this IServiceCollection services)
        {
            services.AddSingleton<IUserAuthService, UsersService>();
            services.AddSingleton<IUserManagementService, UsersService>();
            services.AddSingleton<IUserInfoService, UsersService>();
            services.AddSingleton<IGroupsClient, GroupsClient>();
            services.AddSingleton<IGroupsRolesClient, GroupsRolesClient>();
            services.AddSingleton<ITokensClient, TokensClient>();
            services.AddSingleton<ISprintsClient, SprintsClient>();
            services.AddSingleton<IFeedbacksClient, FeedbacksClient>();
            services.AddSingleton<ITasksClient, TasksClient>();
            services.AddSingleton<IUserClients, UserClients>();

            return services;
        }

        public static async Task SetUserIndexesAsync(this IServiceProvider serviceProvider, string collectionName)
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(SetUserIndexesAsync));

            try
            {
                var collection = serviceProvider.GetRequiredService<IMongoDatabase>()
                    .GetCollection<User>(collectionName);

                var groupName_indexModel = new CreateIndexModel<User>(
                    Builders<User>.IndexKeys.Ascending(u => u.GroupName)
                );

                var username_indexModel = new CreateIndexModel<User>(
                    Builders<User>.IndexKeys.Ascending(u => u.Username),
                    new CreateIndexOptions { Unique = true }
                );

                var email_indexModel = new CreateIndexModel<User>(
                    Builders<User>.IndexKeys.Ascending(u => u.Email),
                    new CreateIndexOptions<User>
                    {
                        Unique = true,
                        PartialFilterExpression = Builders<User>.Filter.Type(u => u.Email, BsonType.String)
                    }
                );

                await Task.WhenAll(
                    collection.Indexes.CreateOneAsync(groupName_indexModel),
                    collection.Indexes.CreateOneAsync(username_indexModel),
                    collection.Indexes.CreateOneAsync(email_indexModel)
                );

            }
            catch (Exception ex)
            {
                logger.LogError("Fallo al agregar índices en {methodName}.\nExcepción: {Msg}\nStackTrace: {StackTrace}",
                    nameof(SetUserIndexesAsync), ex.Message, ex.StackTrace);
            }


        }
    }
}
