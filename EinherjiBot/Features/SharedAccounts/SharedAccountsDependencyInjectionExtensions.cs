using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.SharedAccounts;
using TehGM.EinherjiBot.SharedAccounts.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SharedAccountsDependencyInjectionExtensions
    {
        public static IServiceCollection AddSharedAccounts(this IServiceCollection services, Action<SharedAccountOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure != null)
                services.Configure(configure);

            services.AddDiscordClient();
            services.AddMongoDB();
            services.AddEntityCaching();
            services.AddBotAudits();
            services.TryAddSingleton<ISharedAccountStore, MongoSharedAccountStore>();
            services.TryAddSingleton<ISharedAccountProvider, SharedAccountProvider>();

            return services;
        }
    }
}
