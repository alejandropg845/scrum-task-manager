using MongoDB.Driver;
using RabbitMQ.Client;
using System.Threading.Channels;
using TaskManager.Common.Configurations;
using TaskManager.Common.Documents;
using TaskManager.Common.Interfaces;
using TaskManager.Common.Services;
using TaskManager.Groups.Clients;
using TaskManager.Groups.Interfaces;
using TaskManager.Groups.Repositories;
using TaskManager.Groups.Services;

namespace TaskManager.Groups
{
    public static class Extensions
    {
        public static IServiceCollection SetMongoConfiguration(this IServiceCollection services, MongoDbSettings x)
        {
            services.AddSingleton<IMongoDatabase>(serviceProvider =>
            {
                return new MongoClient(x.ConnectionString).GetDatabase(x.DatabaseName);
            });

            return services;
        }

        public static IServiceCollection AddGroupsRepository(this IServiceCollection services, string collectionName)
        {

            services.AddSingleton(serviceProvider =>
            {
                var collection = serviceProvider
                .GetRequiredService<IMongoDatabase>().GetCollection<Group>(collectionName);
                return new GroupRepository(collection);
            });

            services.AddSingleton<IGroupWriteRepository>(sp => sp.GetRequiredService<GroupRepository>());
            services.AddSingleton<IGroupReadRepository>(sp => sp.GetRequiredService<GroupRepository>());

            return services;
        }
        public static IServiceCollection AddGroupsServiceRequiredParameters(this IServiceCollection services)
        {
            services.AddSingleton<IGroupWriteService, GroupsService>();
            services.AddSingleton<IGroupReadService, GroupsService>();

            services.AddSingleton<ISprintsClient, SprintsClient>();

            services.AddSingleton<IGroupsRolesClient, GroupsRolesClient>();
            services.AddSingleton<IUsersClient, UsersClient>();
            services.AddSingleton<IMessageBusClient, MessageBusClient>();
                
            return services;
        }

        public static string RandomizeGroupName(string groupName)
        {
            string letters = "abcdefghijklmnopqrstvwxyzABCDEFGHIJKLMNOPQRSTVWXYZ1234567890";
            var random = new Random();
            string randomizedGroupName = groupName;

            for (int i = 0; i < 5; i++)
            {
                var letter = letters[random.Next(letters.Length)];
                randomizedGroupName += letter;
            }

            return randomizedGroupName;

        }

        public static async Task SetGroupIndexesAsync(this IServiceProvider serviceProvider, string collectionName)
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(SetGroupIndexesAsync));

            try
            {
                var collection = serviceProvider.GetRequiredService<IMongoDatabase>()
                    .GetCollection<Group>(collectionName);

                var groupName_OwnerName_IndexModel = new CreateIndexModel<Group>(
                    Builders<Group>.IndexKeys
                    .Ascending(g => g.Name)
                    .Ascending(g => g.OwnerName)
                );

                var groupNameIndexModel = new CreateIndexModel<Group>(
                    Builders<Group>.IndexKeys.Ascending(g => g.Name),
                    new CreateIndexOptions { Unique = true }
                );

                var groupName_OwnerName_T = collection.Indexes.CreateOneAsync(groupName_OwnerName_IndexModel);
                var groupName_T = collection.Indexes.CreateOneAsync(groupNameIndexModel);

                await Task.WhenAll(groupName_OwnerName_T, groupName_T);

            }
            catch (Exception ex)
            {
                logger.LogError("Fallo al agregar índices en {methodName}.\nExcepción: {Msg}\nStackTrace: {StackTrace}",
                    nameof(SetGroupIndexesAsync), ex.Message, ex.StackTrace);
            }

        }

    }
}
