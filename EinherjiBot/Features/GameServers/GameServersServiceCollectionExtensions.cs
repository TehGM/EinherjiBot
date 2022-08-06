using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.GameServers;
using TehGM.EinherjiBot.GameServers.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GameServersServiceCollectionExtensions
    {
        public static IServiceCollection AddGameServers(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddLocking();
            services.AddDiscordClient();
            services.AddMongoDB();
            services.AddEntityCaching();
            services.TryAddSingleton<IGameServerStore, MongoGameServerStore>();
            services.TryAddScoped<IGameServerProvider, GameServerProvider>();

            return services;
        }
    }
}
