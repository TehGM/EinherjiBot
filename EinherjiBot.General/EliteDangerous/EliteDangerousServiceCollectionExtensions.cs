using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.EliteDangerous;
using TehGM.EinherjiBot.EliteDangerous.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EliteDangerousServiceCollectionExtensions
    {
        public static IServiceCollection AddEliteCommunityGoals(this IServiceCollection services, Action<CommunityGoalsOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure != null)
                services.Configure(configure);

            services.AddMongoDB();
            services.AddEntityCaching();
            services.TryAddSingleton<ICommunityGoalsHistoryStore, MongoCommunityGoalsHistoryStore>();

            return services;
        }
    }
}
