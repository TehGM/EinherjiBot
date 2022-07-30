using TehGM.EinherjiBot.Database;
using TehGM.EinherjiBot.Database.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DatabaseDependencyInjectionExtensions
    {
        private static bool _guidRegistered = false;

        public static IServiceCollection AddMongoDB(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (!_guidRegistered)
            {
                #pragma warning disable CS0618 // Type or member is obsolete
                BsonDefaults.GuidRepresentationMode = GuidRepresentationMode.V3;
                #pragma warning restore CS0618 // Type or member is obsolete
                BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
                BsonSerializer.RegisterSerializer(new NullableSerializer<Guid>(new GuidSerializer(GuidRepresentation.Standard)));
                _guidRegistered = true;
            }

            services.TryAddSingleton<IMongoConnection, MongoDatabaseClient>();

            return services;
        }
    }
}
