using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.PlaceholdersEngine.API;
using TehGM.EinherjiBot.UI.PlaceholdersEngine.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PlaceholdersEngineServiceCollectionExtensions
    {
        public static IServiceCollection AddPlaceholdersEngineFrontend(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddTransient<IPlaceholdersService, WebPlaceholdersService>();

            return services;
        }
    }
}
