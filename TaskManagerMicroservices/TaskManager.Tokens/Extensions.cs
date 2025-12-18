using MongoDB.Driver;
using TaskManager.Common.Configurations;
using TaskManager.Common.Documents;
using TaskManager.Common.Interfaces;
using TaskManager.Common.Services;
using TaskManager.Tokens.Interfaces;
using TaskManager.Tokens.Repositories;
using TaskManager.Tokens.Services;

namespace TaskManager.Tokens
{
    public static class Extensions
    {
        public static IServiceCollection SetMongoClient(this IServiceCollection services, MongoDbSettings settings)
        {
            var mongoClient = new MongoClient(settings.ConnectionString);

            services.AddSingleton<IMongoClient>(mongoClient);

            services.AddSingleton<IMongoDatabase>(serviceProvider => {
                return mongoClient.GetDatabase(settings.DatabaseName);
            });

            return services;

        }

        public static IServiceCollection AddTokensRepository(this IServiceCollection services, string collectionName)
        {

            services.AddSingleton<ITokensRepository, TokensRepository>(serviceProvider =>
            {
                var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
                var mongoDatabase = serviceProvider.GetRequiredService<IMongoDatabase>();
                var tokensCollection = mongoDatabase.GetCollection<Token>(collectionName);
                return new TokensRepository(tokensCollection);
            });

            return services;
        }

        public static IServiceCollection AddTokensService(this IServiceCollection services)
        {
            services.AddSingleton<ITokensService, TokensService>();
            // ==> ITokensRepository ya añadido.
            services.AddSingleton<ITokenService, TokenService>();
            // ==> IMongoClient ya añadido.

            return services;
        }

        public static async Task SetTokensIndexesAsync(this IServiceProvider serviceProvider, string collectionName)
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(SetTokensIndexesAsync));

            try
            {
                var collection = serviceProvider.GetRequiredService<IMongoDatabase>()
                    .GetCollection<Token>(collectionName);

                var refreshToken_indexModel = new CreateIndexModel<Token>(
                    Builders<Token>.IndexKeys.Ascending(rt => rt.RefreshToken),
                    new CreateIndexOptions { Unique = true }
                );

                await collection.Indexes.CreateOneAsync(refreshToken_indexModel);

            }
            catch (Exception ex)
            {
                logger.LogError("Fallo al agregar índices en {methodName}.\nExcepción: {Msg}\nStackTrace: {StackTrace}",
                    nameof(SetTokensIndexesAsync), ex.Message, ex.StackTrace);
            }
        }
    }
}
