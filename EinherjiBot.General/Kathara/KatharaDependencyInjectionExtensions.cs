using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KatharaDependencyInjectionExtensions
    {
        public static IServiceCollection AddPihole(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddDiscordClient();
            services.AddHttpClient();

            return services;
        }
    }
}
