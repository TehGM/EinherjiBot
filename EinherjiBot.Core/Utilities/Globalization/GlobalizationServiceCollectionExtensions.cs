using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.Globalization;
using TehGM.EinherjiBot.Globalization.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GlobalizationServiceCollectionExtensions
    {
        public static IServiceCollection AddGlobalization(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddTransient<ITimestampFormatter, TimestampFormatter>();

            return services;
        }
    }
}
