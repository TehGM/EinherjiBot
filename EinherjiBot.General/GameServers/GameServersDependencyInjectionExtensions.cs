using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.Caching.Services;
using TehGM.EinherjiBot.GameServers;
using TehGM.EinherjiBot.GameServers.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GameServersDependencyInjectionExtensions
    {
        public static IServiceCollection AddGameServers(this IServiceCollection services, Action<GameServersOptions> configure = null, Action<CachingOptions> configureCaching = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure != null)
                services.Configure(configure);
            if (configureCaching != null)
                services.Configure(MongoGameServerStore.CacheOptionName, configureCaching);

            services.AddDiscordClient();
            services.AddMongoConnection();
            services.TryAddSingleton<IGameServerStore, MongoGameServerStore>();
            services.TryAddSingleton<IEntityCache<string, GameServer>, EntityCache<string, GameServer>>();

            return services;
        }
    }
}
