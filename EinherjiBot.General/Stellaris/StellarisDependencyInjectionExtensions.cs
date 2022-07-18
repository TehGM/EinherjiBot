using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;
using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.Caching.Services;
using TehGM.EinherjiBot.Stellaris;
using TehGM.EinherjiBot.Stellaris.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class StellarisDependencyInjectionExtensions
    {
        public static IServiceCollection AddStellaris(this IServiceCollection services, Action<CachingOptions> configureCaching = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configureCaching != null)
                services.Configure(MongoStellarisModsStore.CacheOptionName, configureCaching);

            services.AddDiscordClient();
            services.AddMongoDB();
            services.TryAddSingleton<IEntityCache<ObjectId, StellarisMod>, EntityCache<ObjectId, StellarisMod>>();
            services.TryAddSingleton<IStellarisModsStore, MongoStellarisModsStore>();

            return services;
        }
    }
}
