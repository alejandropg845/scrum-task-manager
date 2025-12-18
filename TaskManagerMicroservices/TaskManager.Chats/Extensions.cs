using MongoDB.Driver;
using TaskManager.Chats.Interfaces;
using TaskManager.Chats.Repositories;
using TaskManager.Chats.Services;
using TaskManager.Common.Configurations;
using TaskManager.Common.Documents;
using TaskManager.Common.Interfaces;

namespace TaskManager.Chats
{
    public static class Extensions
    {
        public static IServiceCollection AddMongoClient(this IServiceCollection services, MongoDbSettings settings)
        {
            var mongoClient = new MongoClient(settings.ConnectionString);
            services.AddSingleton<IMongoClient>(mongoClient);

            services.AddSingleton<IMongoDatabase>(serviceProvider =>
            {
                return mongoClient.GetDatabase(settings.DatabaseName);
            });
            return services;
        }

        public static IServiceCollection AddMessagesDateRepository(this IServiceCollection services)
        {
            services.AddSingleton<IDatesRepository>(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                string messagesDateCollectionName = config[$"{nameof(MongoDbSettings)}:MessagesDateCollectionName"]!;

                var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
                var mongoCollection = serviceProvider.GetRequiredService<IMongoDatabase>().GetCollection<MessagesDate>(messagesDateCollectionName);

                return new DatesRepository(mongoCollection);

            });

            return services;
        }

        public static IServiceCollection AddMessagesRepository(this IServiceCollection services)
        {
            services.AddSingleton<IMessagesRepository>(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                string messagesCollectionName = config[$"{nameof(MongoDbSettings)}:MessageCollectionName"]!;
                
                var mongoCollection = serviceProvider.GetRequiredService<IMongoDatabase>()
                .GetCollection<Message>(messagesCollectionName);

                return new MessagesRepository(mongoCollection);

            });

            return services;
        }
        public static IServiceCollection AddDatesServiceRequiredDependencies(this IServiceCollection services)
        {
            services.AddSingleton<IDatesService, DatesService>();
            services.AddSingleton<IMessagesService, MessagesService>();

            return services;
        }

        public static async Task SetMessagesDateIndexesAsync(this IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(SetMessagesDateIndexesAsync));

            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var mongoDatabase = serviceProvider.GetRequiredService<IMongoDatabase>();

            string messagesDateCollectionName = config[$"{nameof(MongoDbSettings)}:MessagesDateCollectionName"]!;
            string messageCollectionName = config[$"{nameof(MongoDbSettings)}:MessageCollectionName"]!;

            var messagesCollection = mongoDatabase.GetCollection<Message>(messageCollectionName);
            var messagesDateCollection = mongoDatabase.GetCollection<MessagesDate>(messagesDateCollectionName);

            try
            {

                /*Indexes para MessagesDate*/
                
                var messagesDateIndexModel = new CreateIndexModel<MessagesDate>(
                    Builders<MessagesDate>.IndexKeys.Ascending(m => m.GroupName)
                );

                var messagesDate_T = messagesDateCollection.Indexes.CreateOneAsync(messagesDateIndexModel);

                /*Indexes para Messages*/

                var messageIndexModel = new CreateIndexModel<Message>(
                     Builders<Message>.IndexKeys
                    .Ascending(m => m.DateId)
                    .Descending(m => m.MessageTime)
                );

                var messages_T = messagesCollection.Indexes.CreateOneAsync(messageIndexModel);

                await Task.WhenAll(messagesDate_T, messages_T);

            } catch (Exception ex) 
            {
                logger.LogError("Fallo al agregar índices en {methodName}.\nExcepción: {Msg}\nStackTrace: {StackTrace}",
                    nameof(SetMessagesDateIndexesAsync), ex.Message, ex.StackTrace);
            }

        }
    }
}
