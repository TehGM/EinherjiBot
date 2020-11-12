using System;
using TehGM.EinherjiBot.Kathara;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KatharaDependencyInjectionExtensions
    {
        public static IServiceCollection AddPihole(this IServiceCollection services, Action<PiholeOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure != null)
                services.Configure(configure);

            services.AddDiscordClient();
            services.AddHttpClient();

            return services;
        }
    }
}
