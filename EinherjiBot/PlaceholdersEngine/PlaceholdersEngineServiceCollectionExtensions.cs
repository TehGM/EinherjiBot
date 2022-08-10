using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.PlaceholdersEngine.Services;
using TehGM.EinherjiBot.PlaceholdersEngine;
using TehGM.EinherjiBot.PlaceholdersEngine.API.Services;
using TehGM.EinherjiBot.PlaceholdersEngine.API;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PlaceholdersEngineServiceCollectionExtensions
    {
        public static IServiceCollection AddPlaceholdersEngineBackend(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<IPlaceholdersProvider>(services =>
            {
                PlaceholdersProvider provider = ActivatorUtilities.CreateInstance<PlaceholdersProvider>(services);
                ILogger log = services.GetRequiredService<ILogger<PlaceholdersProvider>>();

                log.LogDebug("Loading placeholders");
                int count = provider.AddPlaceholders(new[] { Assembly.GetExecutingAssembly(), typeof(PlaceholderAttribute).Assembly });
                log.LogInformation("Loaded {Count} placeholders", count);
                return provider;
            });
            services.TryAddTransient<IPlaceholdersEngine, PlaceholdersEngineService>();

            services.TryAddTransient<IPlaceholdersService, ApiPlaceholdersService>();
            services.TryAddTransient<IPlaceholderSerializer, PlaceholderSerializer>();

            return services;
        }
    }
}
