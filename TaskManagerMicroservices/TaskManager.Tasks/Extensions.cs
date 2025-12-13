using MongoDB.Driver;
using Polly;
using TaskManager.Common.Configurations;
using TaskManager.Common.Documents;
using TaskManager.Common.Interfaces;
using TaskManager.Tasks.Clients;
using TaskManager.Tasks.Interfaces;
using TaskManager.Tasks.Repositories;
using TaskManager.Tasks.Services;

namespace TaskManager.Tasks
{
    public static class Extensions
    {
        public static IServiceCollection SetMongoConfiguration(this IServiceCollection services, MongoDbSettings settings)
        {
            services.AddSingleton<IMongoDatabase>(serviceProvider =>
            {
                return new MongoClient(settings.ConnectionString).GetDatabase(settings.DatabaseName);
            });

            return services;
        }

        public static IServiceCollection AddTasksRepository(this IServiceCollection services, string collectionName)
        {
            services.AddSingleton(serviceProvider =>
            {
                var collection = serviceProvider.GetRequiredService<IMongoDatabase>()
                .GetCollection<UserTask>(collectionName);
                
                return new TasksRepository(collection);
            });

            services.AddSingleton<ITaskWriteRepository>(sp => sp.GetRequiredService<TasksRepository>());
            services.AddSingleton<ITaskReadRepository>(sp => sp.GetRequiredService<TasksRepository>());


            return services;
        }
        public static IServiceCollection AddTasksService(this IServiceCollection services)
        {
            services.AddSingleton<ITaskWriteService, TasksService>();
            services.AddSingleton<ITaskReadService, TasksService>();
            // ==> ITasksRepository ya se encuentra agregado.
            services.AddSingleton<ITaskItemsClient, TaskItemsClient>();
            services.AddSingleton<IGroupsClient, GroupsClient>();
            services.AddSingleton<IGroupsRolesClient, GroupsRolesClient>();
            services.AddSingleton<ITaskClients, TaskClients>();
            // => RabbitMqConnection y MessageBus ya registrados en Program.

            return services;
        }

        public static async Task SetTaskIndexesAsync(this IServiceProvider serviceProvider, string mongoCollection)
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(SetTaskIndexesAsync));

            try
            {
                var collection = serviceProvider.GetRequiredService<IMongoDatabase>()
                    .GetCollection<UserTask>(mongoCollection);

                var groupName_indexModel = new CreateIndexModel<UserTask>(
                    Builders<UserTask>.IndexKeys.Ascending(t => t.GroupName)
                );

                var sprintId_indexModel = new CreateIndexModel<UserTask>(
                    Builders<UserTask>.IndexKeys.Ascending(t => t.SprintId)
                );

                var groupName_username_indexModel = new CreateIndexModel<UserTask>(
                    Builders<UserTask>.IndexKeys
                    .Ascending(t => t.GroupName)
                    .Ascending(t => t.Username)
                );

                await Task.WhenAll(
                    collection.Indexes.CreateOneAsync(groupName_indexModel),
                    collection.Indexes.CreateOneAsync(sprintId_indexModel),
                    collection.Indexes.CreateOneAsync(groupName_username_indexModel)
                );

            } catch (Exception ex)
            {
                logger.LogError("Fallo al agregar índices en {methodName}.\nExcepción: {Msg}\nStackTrace: {StackTrace}",
                    nameof(SetTaskIndexesAsync), ex.Message, ex.StackTrace);
            }
        }

    }
}
