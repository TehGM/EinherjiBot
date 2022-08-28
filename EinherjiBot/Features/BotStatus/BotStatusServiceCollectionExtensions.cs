using TehGM.EinherjiBot.BotStatus;
using TehGM.EinherjiBot.BotStatus.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class BotStatusServiceCollectionExtensions
    {
        public static IServiceCollection AddBotStatusBackend(this IServiceCollection services, Action<BotStatusOptions> configureOptions = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configureOptions != null)
                services.Configure(configureOptions);

            services.AddLocking();
            services.AddPlaceholdersEngineBackend();
            services.TryAddSingleton<IStatusStore, MongoStatusStore>();
            services.TryAddScoped<IBotStatusProvider, BotStatusProvider>();
            services.TryAddScoped<IBotStatusSetter, BotStatusSetter>();
            services.AddHostedService<AutoStatusService>();

            services.TryAddTransient<IBotStatusHandler, ServerBotStatusHandler>();

            return services;
        }
    }
}
