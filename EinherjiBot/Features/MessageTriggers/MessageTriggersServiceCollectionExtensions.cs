using Microsoft.Extensions.DependencyInjection.Extensions;
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

            return services;
        }
    }
}
