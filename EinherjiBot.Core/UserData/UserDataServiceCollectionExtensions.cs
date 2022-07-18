using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot;
using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.Caching.Services;
using TehGM.EinherjiBot.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class UserDataServiceCollectionExtensions
    {
        public static IServiceCollection AddUserData(this IServiceCollection services, Action<CachingOptions> configureCaching = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configureCaching != null)
                services.Configure(MongoUserDataStore.CacheOptionName, configureCaching);

            services.AddMongoConnection();
            services.TryAddSingleton<IEntityCache<ulong, UserData>, EntityCache<ulong, UserData>>();
            services.TryAddSingleton<IUserDataStore, MongoUserDataStore>();

            return services;
        }
    }
}
