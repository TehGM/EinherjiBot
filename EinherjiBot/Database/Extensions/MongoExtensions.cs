using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace TehGM.EinherjiBot.Database
{
    public static class MongoExtensions
    {
        public static IMongoDatabase GetDatabase(this IMongoConnection client, string name, MongoDatabaseSettings settings = null)
            => client.Client.GetDatabase(name, settings);

        // client is noop - merely for ease of access
        public static void RegisterClassMap<T>(this IMongoConnection client, Action<BsonClassMap<T>> classMapInitializer)
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
            {
                if (classMapInitializer == null)
                    BsonClassMap.RegisterClassMap<T>();
                else
                    BsonClassMap.RegisterClassMap<T>(classMapInitializer);
            }
        }

        // client is noop - merely for ease of access
        public static void RegisterClassMap<T>(this IMongoConnection client)
            => RegisterClassMap<T>(null);
    }
}
