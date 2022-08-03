using TehGM.EinherjiBot.BotStatus;
using TehGM.EinherjiBot.BotStatus.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.BotStatus.API;
using TehGM.EinherjiBot.BotStatus.API.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class BotStatusDependencyInjectionExtensions
    {
        public static IServiceCollection AddBotStatusBackend(this IServiceCollection services, Action<BotStatusOptions> configureOptions = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configureOptions != null)
                services.Configure(configureOptions);

            services.AddPlaceholdersEngineBackend();
            services.TryAddSingleton<IStatusStore, MongoStatusStore>();
            services.TryAddSingleton<IStatusProvider, StatusProvider>();
            services.TryAddSingleton<IStatusService, RandomStatusService>();
            services.AddHostedService<RandomStatusService>(s => (RandomStatusService)s.GetRequiredService<IStatusService>());

            services.TryAddTransient<IBotStatusService, ApiBotStatusService>();

            return services;
        }
    }
}
