using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.Caching.Services;
using TehGM.EinherjiBot.EliteDangerous;
using TehGM.EinherjiBot.EliteDangerous.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EliteDangerousServiceCollectionExtensions
    {
        public static IServiceCollection AddEliteCommunityGoals(this IServiceCollection services, Action<CommunityGoalsOptions> configure = null, Action<CachingOptions> configureCaching = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure != null)
                services.Configure(configure);
            if (configureCaching != null)
                services.Configure(MongoCommunityGoalsHistoryStore.CacheOptionName, configureCaching);

            services.AddMongoDB();
            services.TryAddSingleton<IEntityCache<int, CommunityGoal>, EntityCache<int, CommunityGoal>>();
            services.TryAddSingleton<ICommunityGoalsHistoryStore, MongoCommunityGoalsHistoryStore>();

            return services;
        }
    }
}
