using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.Settings.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SettingsServiceCollectionExtensions
    {
        public static IServiceCollection AddGuildSettingsBackend(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddLocking();
            services.AddDiscordClient();
            services.AddMongoDB();
            services.AddEntityCaching();
            services.AddBotAudits();
            services.TryAddSingleton<IGuildSettingsStore, MongoGuildSettingsStore>();
            services.TryAddTransient<IGuildSettingsProvider, GuildSettingsProvider>();
            services.TryAddTransient<IGuildSettingsHandler, ServerGuildSettingsHandler>();

            return services;
        }
    }
}
