using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.Patchbot;
using TehGM.EinherjiBot.Patchbot.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PatchbotDependencyInjectionExtensions
    {
        public static IServiceCollection AddPatchbot(this IServiceCollection services, Action<PatchbotOptions> configure = null, Action<CachingOptions> configureCaching = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure != null)
                services.Configure(configure);
            if (configureCaching != null)
                services.Configure(MongoPatchbotGameStore.CacheOptionName, configureCaching);

            services.AddDiscordClient();
            services.AddMongoConnection();
            services.TryAddSingleton<IPatchbotGamesStore, MongoPatchbotGameStore>();

            return services;
        }
    }
}
