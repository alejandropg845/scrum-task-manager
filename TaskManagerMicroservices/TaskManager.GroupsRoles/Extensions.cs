using MongoDB.Driver;
using TaskManager.Common.Configurations;
using TaskManager.Common.Documents;
using TaskManager.GroupsRoles.Interfaces;
using TaskManager.GroupsRoles.Repositories;

namespace TaskManager.GroupsRoles
{
    public static class Extensions
    {
        public static IServiceCollection SetMongoConfiguration(this IServiceCollection services, MongoDbSettings x)
        {
            var mongoClient = new MongoClient(x.ConnectionString);

            services.AddSingleton<IMongoClient>(_ => mongoClient);

            services.AddSingleton<IMongoDatabase>(serviceProvider =>
            {
                return mongoClient.GetDatabase(x.DatabaseName);
            });

            return services;
        }
        public static IServiceCollection SetRepository(this IServiceCollection services, string collectionName)
        {
            services.AddSingleton<GroupsRolesRepository>(serviceProvider =>
            {
                var database = serviceProvider.GetRequiredService<IMongoDatabase>();
                var collection = database.GetCollection<Common.Documents.GroupsRoles>(collectionName);
                return new GroupsRolesRepository(collection);
            });
            services.AddSingleton<IGroupRolesReadRepository>(sp => sp.GetRequiredService<GroupsRolesRepository>());
            services.AddSingleton<IGroupRolesWriteRepository>(sp => sp.GetRequiredService<GroupsRolesRepository>());

            return services;
        }


        public static async Task SetGroupRoleIndexesAsync(this IServiceProvider serviceProvider, string collectionName)
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(SetGroupRoleIndexesAsync));

            try
            {
                var collection = serviceProvider.GetRequiredService<IMongoDatabase>().GetCollection<Common.Documents.GroupsRoles>(collectionName);

                var groupName_username_indexModel = new CreateIndexModel<Common.Documents.GroupsRoles>(
                    Builders<Common.Documents.GroupsRoles>.IndexKeys
                    .Ascending(gr => gr.GroupName)
                    .Ascending(gr => gr.UserName),
                    new CreateIndexOptions { Unique = true }
                );

                var groupName_roleName_indexModel = new CreateIndexModel<Common.Documents.GroupsRoles>(
                    Builders<Common.Documents.GroupsRoles>.IndexKeys
                    .Ascending(gr => gr.GroupName)
                    .Ascending(gr => gr.RoleName)
                );

                var groupName_username_T = collection.Indexes.CreateOneAsync(groupName_roleName_indexModel);
                var groupName_roleName_T = collection.Indexes.CreateOneAsync(groupName_roleName_indexModel);

                await Task.WhenAll(groupName_username_T, groupName_roleName_T);

            }
            catch (Exception ex)
            {
                logger.LogError("Fallo al agregar índices en {methodName}.\nExcepción: {Msg}\nStackTrace: {StackTrace}",
                    nameof(SetGroupRoleIndexesAsync), ex.Message, ex.StackTrace);
            }

        }
    }
}
