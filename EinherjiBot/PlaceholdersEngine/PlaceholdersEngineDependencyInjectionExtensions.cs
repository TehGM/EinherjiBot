using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.PlaceholdersEngine.Services;
using TehGM.EinherjiBot.PlaceholdersEngine;
using TehGM.EinherjiBot.PlaceholdersEngine.Placeholders;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PlaceholdersEngineDependencyInjectionExtensions
    {
        public static IServiceCollection AddPlaceholdersEngine(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<IPlaceholdersProvider>(services =>
            {
                PlaceholdersProvider provider = ActivatorUtilities.CreateInstance<PlaceholdersProvider>(services);
                ILogger log = services.GetRequiredService<ILogger<PlaceholdersProvider>>();

                log.LogDebug("Loading all placeholders from current assembly");
                IEnumerable<Type> types = Assembly.GetExecutingAssembly().DefinedTypes.Where(t =>
                        typeof(IPlaceholder).IsAssignableFrom(t) &&
                        !Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute)) &&
                        Attribute.IsDefined(t, typeof(PlaceholderAttribute), true));
                int count = provider.AddPlaceholders(types);
                log.LogInformation("Loaded {Count} placeholders", count);
                return provider;
            });
            services.TryAddTransient<IPlaceholdersEngine, PlaceholdersEngineService>();

            return services;
        }
    }
}
