using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IntelDependencyInjectionExtensions
    {
        public static IServiceCollection AddIntel(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddDiscordClient();
            services.AddMongoConnection();
            services.AddUserData();

            return services;
        }
    }
}
