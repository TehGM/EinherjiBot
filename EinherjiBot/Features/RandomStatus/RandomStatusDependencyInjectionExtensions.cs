using TehGM.EinherjiBot.RandomStatus;
using TehGM.EinherjiBot.RandomStatus.Services;
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

            services.AddPlaceholdersEngine();
            services.TryAddSingleton<IStatusStore, MongoStatusStore>();
            services.TryAddSingleton<IStatusProvider, StatusProvider>();
            services.TryAddSingleton<IStatusService, RandomStatusService>();
            services.AddHostedService<RandomStatusService>(s => (RandomStatusService)s.GetRequiredService<IStatusService>());

            return services;
        }
    }
}
