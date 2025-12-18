using MongoDB.Driver;
using TaskManager.Common.Configurations;
using TaskManager.Common.Documents;
using TaskManager.Common.Interfaces;
using TaskManager.Common.Services;
using TaskManager.Sprints.Clients;
using TaskManager.Sprints.Interfaces;
using TaskManager.Sprints.Repositories;
using TaskManager.Sprints.Services;

namespace TaskManager.Sprints
{
    public static class Extensions
    {
        public static IServiceCollection SetMongoConfiguration(this IServiceCollection services, MongoDbSettings s)
        {
            var mongoClient = new MongoClient(s.ConnectionString);

            services.AddSingleton<IMongoClient>(_ => mongoClient);

            services.AddSingleton<IMongoDatabase>(serviceProvider =>
            {
                return mongoClient.GetDatabase(s.DatabaseName);
            });
            return services;
        }

        public static IServiceCollection AddSprintsRepository(this IServiceCollection services, MongoDbSettings s)
        {
            services.AddSingleton(serviceProvider =>
            {
                var collection = serviceProvider
                .GetRequiredService<IMongoDatabase>().GetCollection<Sprint>(s.CollectionName);
                return new SprintsRepository(collection);
            });

            services.AddSingleton<ISprintReadRepository>(sp => sp.GetRequiredService<SprintsRepository>());
            services.AddSingleton<ISprintWriteRepository>(sp => sp.GetRequiredService<SprintsRepository>());

            return services;
        }
        public static IServiceCollection AddRetrosRepository(this IServiceCollection services, string collectionName)
        {
            services.AddSingleton<IRetrospectivesRepository>(serviceProvider =>
            {
                var collection = serviceProvider
                .GetRequiredService<IMongoDatabase>().GetCollection<SprintRetrospective>(collectionName);

                return new RetrospectivesRepository(collection);
            });

            return services;
        }

        public static IServiceCollection AddFeedbackRepository(this IServiceCollection services, string collectionName)
        {
            services.AddSingleton<IFeedbackRepository>(serviceProvider =>
            {
                var mongoCollection = serviceProvider.GetRequiredService<IMongoDatabase>()
                .GetCollection<Feedback>(collectionName);

                return new FeedbackRepository(mongoCollection);

            });

            return services;
        }

        public static IServiceCollection AddSprintsServiceRequiredAbstractions(this IServiceCollection services)
        {
            services.AddSingleton<IRetrospectivesService, RetrospectivesService>();
            services.AddSingleton<IUsersClient, UsersClient>();
            services.AddSingleton<ISprintWriteService, SprintsService>();
            services.AddSingleton<ISprintReadService, SprintsService>();
            services.AddSingleton<IMessageBusClient, MessageBusClient>();
            services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
            services.AddSingleton<IGroupsRolesClient, GroupsRolesClient>();
            services.AddSingleton<ITasksClient, TasksClient>();

            return services;
        }

        public static async Task SetSprintIndexesAsync(
            this IServiceProvider serviceProvider,
            string sprintsCollectionName,
            string feedbacksCollectionName,
            string retrosCollectionName
        )
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(SetSprintIndexesAsync));

            var mongoDatabase = serviceProvider.GetRequiredService<IMongoDatabase>();

            try
            {
                var sprintsCollection = mongoDatabase.GetCollection<Sprint>(sprintsCollectionName);
                var feedbacksCollection = mongoDatabase.GetCollection<Feedback>(feedbacksCollectionName);
                var retrosCollection = mongoDatabase.GetCollection<SprintRetrospective>(retrosCollectionName);

                /* Agregar indexes a Sprints */
                var (task1, task2) = SetSprintsIndexesAsync(sprintsCollection);

                var (task3, task4) = SetFeedbackIndexesAsync(feedbacksCollection);

                var task5 = SetRetroIndexesAsync(retrosCollection);

                await Task.WhenAll(task1, task2, task3, task4, task5);

            } catch (Exception ex)
            {
                logger.LogError("Fallo al agregar índices en {methodName}.\nExcepción: {Msg}\nStackTrace: {StackTrace}",
                    nameof(SetSprintIndexesAsync), ex.Message, ex.StackTrace);
            }
        }

        private static (Task x, Task y) SetSprintsIndexesAsync(IMongoCollection<Sprint> collection)
        {
            var groupName_sprintNumber_indexModel = new CreateIndexModel<Sprint>(
                Builders<Sprint>.IndexKeys
                .Ascending(s => s.GroupName)
                .Descending(s => s.SprintNumber),
                new CreateIndexOptions { Unique = true }
            );

            var groupName_status_indexModel = new CreateIndexModel<Sprint>(
                Builders<Sprint>.IndexKeys
                .Ascending(s => s.GroupName)
                .Ascending(s => s.Status)
            );

            var groupName_sprintNumber_T = collection.Indexes.CreateOneAsync(groupName_sprintNumber_indexModel);
            var groupName_status_T = collection.Indexes.CreateOneAsync(groupName_status_indexModel);
            
            return new(
                groupName_sprintNumber_T,
                groupName_status_T
            );
        }

        private static (Task x, Task y) SetFeedbackIndexesAsync(IMongoCollection<Feedback> collection)
        {
            var groupName_username_sprintId_indexModel = new CreateIndexModel<Feedback>(
                Builders<Feedback>.IndexKeys
                .Ascending(f => f.GroupName)
                .Ascending(f => f.Username)
                .Ascending(f => f.SprintId),
                new CreateIndexOptions { Unique = true }
            );

            var groupName_sprintId_indexModel = new CreateIndexModel<Feedback>(
                Builders<Feedback>.IndexKeys
                .Ascending(f => f.GroupName)
                .Ascending(f => f.SprintId)
            );

            return new(
                collection.Indexes.CreateOneAsync(groupName_username_sprintId_indexModel),
                collection.Indexes.CreateOneAsync(groupName_sprintId_indexModel)
            );
        }

        private static Task SetRetroIndexesAsync(IMongoCollection<SprintRetrospective> collection)
        {
            var groupName_indexModel = new CreateIndexModel<SprintRetrospective>(
                Builders<SprintRetrospective>.IndexKeys.Ascending(r => r.GroupName)
            );

            var groupName_index_T = collection.Indexes.CreateOneAsync(groupName_indexModel);

            return groupName_index_T;
        }
    }
}
