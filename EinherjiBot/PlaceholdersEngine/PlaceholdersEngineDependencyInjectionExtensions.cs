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

            services.TryAddSingleton<IPlaceholdersEngine>(services =>
            {
                PlaceholdersEngineService engine = ActivatorUtilities.CreateInstance<PlaceholdersEngineService>(services);
                ILogger log = services.GetRequiredService<ILogger<PlaceholdersEngineService>>();

                log.LogDebug("Loading all status placeholders from current assembly");
                IEnumerable<Type> types = Assembly.GetExecutingAssembly().DefinedTypes.Where(t =>
                        typeof(IPlaceholder).IsAssignableFrom(t) &&
                        !Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute)) &&
                        Attribute.IsDefined(t, typeof(PlaceholderAttribute), true));
                int count = engine.AddPlaceholders(types);
                log.LogInformation("Loaded {Count} status placeholders", count);
                return engine;
            });

            return services;
        }
    }
}
