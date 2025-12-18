using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Polly;
using TaskManager.Common.Configurations;
using TaskManager.Common.Documents;
using TaskManager.Common.Interfaces;
using TaskManager.Common.Services;
using TaskManager.TaskItems.Clients;
using TaskManager.TaskItems.Interfaces;
using TaskManager.TaskItems.Repositories;
using TaskManager.TaskItems.Services;

namespace TaskManager.TaskItems
{
    public static class Extensions
    {
        public static IServiceCollection SetMongoConfiguration(this IServiceCollection services, MongoDbSettings settings)
        {
            services.AddSingleton<IMongoDatabase>(servideProvider =>
            {
                var mongoClient = new MongoClient(settings.ConnectionString);

                return mongoClient.GetDatabase(settings.DatabaseName);
            });

            return services;
        }

        public static IServiceCollection AddTaskItemsRepository(this IServiceCollection services, string collectionName)
        {
            services.AddSingleton(serviceProvider =>
            {
                var collection = serviceProvider
                .GetRequiredService<IMongoDatabase>().GetCollection<TaskItem>(collectionName);
                return new TaskItemsRepository(collection);
            });

            services.AddSingleton<ITaskItemsWriteRepository>(sp => sp.GetRequiredService<TaskItemsRepository>());
            services.AddSingleton<ITaskItemsReadRepository>(sp => sp.GetRequiredService<TaskItemsRepository>());
            return services;
        }

        public static IServiceCollection AddTaskItemsService(this IServiceCollection services)
        {
            services.AddSingleton<ITaskItemsWriteService, TaskItemsService>();
            services.AddSingleton<ITaskItemsReadService, TaskItemsService>();

            // ==> TaskITemsRepo ya se encuentra agregado
            services.AddSingleton<ITasksClient, TasksClient>();
            services.AddSingleton<IGroupsClient, GroupsClient>();
            services.AddSingleton<IGroupsRolesClient, GroupsRolesClient>();
            services.AddSingleton<ISprintsClient, SprintsClient>();
            services.AddSingleton<ITaskItemClients, TaskItemClients>();
            // ==> IMessageBus y IRabbitMqConnection ya se encuentra agregados en el Program.
            // ==> IGeminiClient ya se encuentra agregado en el Program.

            return services;

        }

        public static async Task SetTaskItemIndexesAsync(this IServiceProvider serviceProvider, string collectionName)
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(SetTaskItemIndexesAsync));
            var collection = serviceProvider.GetRequiredService<IMongoDatabase>()
                .GetCollection<TaskItem>(collectionName);

            try
            {

                var assignToUsername_groupName_indexModel = new CreateIndexModel<TaskItem>(
                    Builders<TaskItem>.IndexKeys
                    .Ascending(ti => ti.AssignToUsername)
                    .Ascending(ti => ti.GroupName)
                );

                var taskId_indexModel = new CreateIndexModel<TaskItem>(
                    Builders<TaskItem>.IndexKeys
                    .Ascending(ti => ti.TaskId)
                );

                await Task.WhenAll(
                    collection.Indexes.CreateOneAsync(assignToUsername_groupName_indexModel),
                    collection.Indexes.CreateOneAsync(taskId_indexModel)
                );

            } catch (Exception ex) 
            {
                logger.LogError("Fallo al agregar índices en {methodName}.\nExcepción: {Msg}\nStackTrace: {StackTrace}",
                    nameof(SetTaskItemIndexesAsync), ex.Message, ex.StackTrace);
            }
        }
    }
}
