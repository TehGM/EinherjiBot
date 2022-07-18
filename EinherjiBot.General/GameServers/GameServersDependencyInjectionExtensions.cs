using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.GameServers;
using TehGM.EinherjiBot.GameServers.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GameServersDependencyInjectionExtensions
    {
        public static IServiceCollection AddGameServers(this IServiceCollection services, Action<GameServersOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure != null)
                services.Configure(configure);

            services.AddDiscordClient();
            services.AddMongoDB();
            services.AddEntityCaching();
            services.TryAddSingleton<IGameServerStore, MongoGameServerStore>();

            return services;
        }
    }
}
