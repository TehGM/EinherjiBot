using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AdministrationDependencyInjectionExtensions
    {
        public static IServiceCollection AddAdministration(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddDiscordClient();

            return services;
        }
    }
}
