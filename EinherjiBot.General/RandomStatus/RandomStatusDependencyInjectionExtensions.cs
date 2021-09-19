using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.RandomStatus;
using TehGM.EinherjiBot.RandomStatus.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RandomStatusDependencyInjectionExtensions
    {
        public static IServiceCollection AddRandomStatus(this IServiceCollection services, Action<RandomStatusOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure != null)
                services.Configure(configure);

            services.AddDiscordClient();
            services.AddMongoConnection();
            services.TryAddSingleton<IAdvancedStatusConverter, AdvancedStatusConverter>();

            return services;
        }
    }
}
