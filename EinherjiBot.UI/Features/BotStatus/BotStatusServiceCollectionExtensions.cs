using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.BotStatus.API;
using TehGM.EinherjiBot.UI.BotStatus.API;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class BotStatusServiceCollectionExtensions
    {
        public static IServiceCollection AddBotStatusFrontend(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddTransient<IBotStatusService, WebBotStatusService>();

            return services;
        }
    }
}
