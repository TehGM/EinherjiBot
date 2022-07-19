using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.Security;
using TehGM.EinherjiBot.Security.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class UserContextServiceCollectionExtensions
    {
        public static IServiceCollection AddUserContext(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddDiscordClient();
            services.AddMongoDB();
            services.AddEntityCaching();
            services.TryAddSingleton<IUserSecurityDataStore, MongoUserSecurityDataStore>();
            services.TryAddSingleton<IUserContextProvider, DiscordSocketUserContextProvider>();

            return services;
        }
    }
}
