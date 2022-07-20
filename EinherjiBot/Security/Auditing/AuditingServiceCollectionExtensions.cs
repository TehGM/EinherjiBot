using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using System.Reflection;
using System.Runtime.CompilerServices;
using TehGM.EinherjiBot.Auditing;
using TehGM.EinherjiBot.Auditing.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuditingServiceCollectionExtensions
    {
        public static IServiceCollection AddBotAudits(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddMongoDB();
            services.TryAddSingleton(typeof(IAuditStore<>), typeof(MongoAuditStore<>));

            MapAuditTypes();

            return services;
        }

        private static void MapAuditTypes()
        {
            IEnumerable<Type> auditTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(BotAuditEntry).IsAssignableFrom(t)
                    && !t.IsAbstract
                    && !Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute)));
            foreach (Type type in auditTypes)
            {
                if (BsonClassMap.IsClassMapRegistered(type))
                    continue;

                string discriminator = type.GetCustomAttribute<BsonDiscriminatorAttribute>()?.Discriminator
                    ?? type.Name;
                BsonClassMap map = new BsonClassMap(type);
                map.SetDiscriminatorIsRequired(true);
                map.SetDiscriminator(discriminator);
                BsonClassMap.RegisterClassMap(map);
            }
        }
    }
}
