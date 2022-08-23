using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.UI.API;
using TehGM.EinherjiBot.UI.API.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DiscordEntityInfoServiceCollectionExtensions
    {
        public static IServiceCollection AddEntityInfoFrontend(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddLocking();
            services.TryAddScoped<IDiscordEntityInfoService, WebDiscordEntityInfoService>();
            services.TryAddScoped<IDiscordEntityInfoCache, WebDiscordEntityInfoCache>();

            return services;
        }
    }
}
