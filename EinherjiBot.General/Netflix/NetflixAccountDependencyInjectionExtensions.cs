using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.Netflix;
using TehGM.EinherjiBot.Netflix.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NetflixAccountDependencyInjectionExtensions
    {
        public static IServiceCollection AddNetflixAccount(this IServiceCollection services, Action<NetflixAccountOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure != null)
                services.Configure(configure);

            services.AddDiscordClient();
            services.AddMongoDB();
            services.TryAddSingleton<INetflixAccountStore, MongoNetflixAccountStore>();

            return services;
        }
    }
}
