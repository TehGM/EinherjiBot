using TehGM.EinherjiBot.BotStatus;
using TehGM.EinherjiBot.BotStatus.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.BotStatus.API;
using TehGM.EinherjiBot.BotStatus.API.Services;

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
            services.TryAddScoped<IStatusProvider, StatusProvider>();
            services.TryAddScoped<IBotStatusSetter, BotStatusSetter>();
            services.AddHostedService<AutoStatusService>();

            services.TryAddTransient<IBotStatusService, ApiBotStatusService>();

            return services;
        }
    }
}
