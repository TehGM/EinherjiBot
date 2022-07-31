using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using System.Reflection;
using System.Runtime.CompilerServices;
using TehGM.EinherjiBot.MessageTriggers;
using TehGM.EinherjiBot.MessageTriggers.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MessageTriggersServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageTriggers(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddDiscordClient();
            services.AddMongoDB();
            services.AddEntityCaching();
            services.TryAddSingleton<IMessageTriggersStore, MongoMessageTriggersStore>();
            services.TryAddSingleton<IMessageTriggersProvider, MessageTriggersProvider>();
            services.AddHostedService<DiscordMessageTriggersListener>();

            MapActions();

            return services;
        }

        private static void MapActions()
        {
            IEnumerable<Type> actionTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IMessageTriggerAction).IsAssignableFrom(t)
                    && !t.IsAbstract
                    && !Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute)));
            foreach (Type type in actionTypes)
            {
                if (BsonClassMap.IsClassMapRegistered(type))
                    continue;

                BsonClassMap map = new BsonClassMap(type);
                map.AutoMap();
                map.SetDiscriminatorIsRequired(true);
                map.SetDiscriminator(GetDiscriminator(type));
                BsonClassMap.RegisterClassMap(map);
            }

            string GetDiscriminator(Type type)
            {
                string result = type.GetCustomAttribute<BsonDiscriminatorAttribute>()?.Discriminator;
                if (!string.IsNullOrWhiteSpace(result))
                    return result;

                string name = type.Name;
                if (name.EndsWith("Action", StringComparison.Ordinal))
                    name = name[0..^6];
                return $"MessageTriggers.Actions.{name}";
            }
        }
    }
}
