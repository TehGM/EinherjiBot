using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot;
using TehGM.EinherjiBot.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LockingServiceCollectionExtensions
    {
        public static IServiceCollection AddLocking(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton(typeof(ILockProvider<>), typeof(LockProvider<>));

            return services;
        }
    }
}
