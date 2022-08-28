using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.API.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DiscordEntityInfoServiceCollectionExtensions
    {
        public static IServiceCollection AddEntityInfoBackend(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddDiscordClient();

            services.TryAddTransient<IDiscordEntityInfoProvider, ServerDiscordEntityInfoProvider>();

            return services;
        }
    }
}
