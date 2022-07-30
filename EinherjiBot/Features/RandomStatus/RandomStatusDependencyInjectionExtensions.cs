using TehGM.EinherjiBot.RandomStatus;
using TehGM.EinherjiBot.RandomStatus.Placeholders;
using TehGM.EinherjiBot.RandomStatus.Services;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RandomStatusDependencyInjectionExtensions
    {
        public static IServiceCollection AddRandomStatus(this IServiceCollection services, Action<RandomStatusOptions> configureOptions = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configureOptions != null)
                services.Configure(configureOptions);

            services.TryAddSingleton<IStatusPlaceholderEngine>(services =>
            {
                StatusPlaceholderEngine engine = ActivatorUtilities.CreateInstance<StatusPlaceholderEngine>(services);
                ILogger log = services.GetRequiredService<ILogger<StatusPlaceholderEngine>>();

                log.LogDebug("Loading all status placeholders from current assembly");
                IEnumerable<Type> types = Assembly.GetExecutingAssembly().DefinedTypes.Where(t =>
                        typeof(IStatusPlaceholder).IsAssignableFrom(t) &&
                        !Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute)) &&
                        Attribute.IsDefined(t, typeof(StatusPlaceholderAttribute), true));
                int count = engine.AddPlaceholders(types);
                log.LogInformation("Loaded {Count} status placeholders", count);
                return engine;
            });

            services.TryAddSingleton<IStatusStore, MongoStatusStore>();
            services.TryAddSingleton<IStatusProvider, StatusProvider>();
            services.AddHostedService<RandomStatusService>();

            return services;
        }
    }
}
