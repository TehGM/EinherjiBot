using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.SharedAccounts;
using TehGM.EinherjiBot.SharedAccounts.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SharedAccountsServiceCollectionExtensions
    {
        public static IServiceCollection AddSharedAccountsBackend(this IServiceCollection services, Action<SharedAccountOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure != null)
                services.Configure(configure);

            services.AddLocking();
            services.AddDiscordClient();
            services.AddMongoDB();
            services.AddEntityCaching();
            services.AddBotAudits();
            services.TryAddSingleton<ISharedAccountStore, MongoSharedAccountStore>();
            services.TryAddTransient<ISharedAccountProvider, SharedAccountProvider>();
            services.TryAddTransient<ISharedAccountImageProvider, SharedAccountImageProvider>();
            services.TryAddTransient<ISharedAccountHandler, ServerSharedAccountHandler>();

            return services;
        }
    }
}
