using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using TehGM.EinherjiBot.PlaceholdersEngine;
using TehGM.EinherjiBot.PlaceholdersEngine.API;
using TehGM.EinherjiBot.PlaceholdersEngine.Services;
using TehGM.EinherjiBot.UI.PlaceholdersEngine;
using TehGM.EinherjiBot.UI.PlaceholdersEngine.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PlaceholdersEngineServiceCollectionExtensions
    {
        public static IServiceCollection AddPlaceholdersEngineFrontend(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<IPlaceholdersProvider>(services =>
            {
                PlaceholdersProvider provider = ActivatorUtilities.CreateInstance<PlaceholdersProvider>(services);
                ILogger log = services.GetRequiredService<ILogger<PlaceholdersProvider>>();

                log.LogDebug("Loading placeholders");
                int count = provider.AddPlaceholders(new[] { typeof(TehGM.EinherjiBot.UI.Program).Assembly, typeof(PlaceholderAttribute).Assembly }, validateHandlers: false);
                log.LogInformation("Loaded {Count} placeholders", count);
                return provider;
            });
            services.TryAddTransient<IPlaceholdersEngine, PlaceholdersEngineService>();

            services.TryAddTransient<IPlaceholdersService, WebPlaceholdersService>();
            services.TryAddTransient<IPlaceholderSerializer, PlaceholderSerializer>();
            services.TryAddTransient<IPlaceholdersBuilder, PlaceholdersBuilderOpener>();

            return services;
        }
    }
}
