using System;
using TehGM.EinherjiBot.Administration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AdministrationDependencyInjectionExtensions
    {
        public static IServiceCollection AddAdministration(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddDiscordClient();

            return services;
        }

        public static IServiceCollection AddBotChannelsRedirection(this IServiceCollection services, Action<BotChannelsRedirectionOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure != null)
                services.Configure(configure);

            services.AddDiscordClient();

            return services;
        }
    }
}
