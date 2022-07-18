using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.Caching.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CachingServiceCollectionExtensions
    {
        public static IServiceCollection AddEntityCaching(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton(typeof(IEntityCache<,>), typeof(EntityCacheFactory<,>));
            services.TryAddSingleton(typeof(IEntityCache<>), typeof(EntityCacheFactory<>));

            return services;
        }
    }
}
