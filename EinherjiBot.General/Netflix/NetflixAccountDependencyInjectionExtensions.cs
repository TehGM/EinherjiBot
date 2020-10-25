using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.Netflix;
using TehGM.EinherjiBot.Netflix.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NetflixAccountDependencyInjectionExtensions
    {
        public static IServiceCollection AddNetflixAccount(this IServiceCollection services, Action<NetflixAccountOptions> configure = null, Action<CachingOptions> configureCaching = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure != null)
                services.Configure(configure);
            if (configureCaching != null)
                services.Configure(NetflixAccountStore.CacheOptionName, configureCaching);

            services.AddMongoConnection();
            services.TryAddSingleton<INetflixAccountStore, NetflixAccountStore>();

            return services;
        }
    }
}
