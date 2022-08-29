using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.Settings;
using TehGM.EinherjiBot.UI.Settings;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SettingsServiceCollectionExtensions
    {
        public static IServiceCollection AddGuildSettingsFrontend(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddTransient<IGuildSettingsHandler, WebGuildSettingsHandler>();

            return services;
        }
    }
}
