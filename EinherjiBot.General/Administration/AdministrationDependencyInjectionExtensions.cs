using System;
using TehGM.EinherjiBot.Administration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AdministrationDependencyInjectionExtensions
    {
        public static IServiceCollection AddBotChannelsRedirection(this IServiceCollection services, Action<BotChannelsRedirectionOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure != null)
                services.Configure(configure);

            services.AddDiscordClient();
            services.AddHostedService<BotChannelsRedirectionHandler>();

            return services;
        }
    }
}
