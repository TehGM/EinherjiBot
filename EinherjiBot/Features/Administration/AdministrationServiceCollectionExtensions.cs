using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.Features.Administration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AdministrationServiceCollectionExtensions
    {
        public static IServiceCollection AddAdministration(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddDiscordClient();
            services.AddHostedService<UserLeaveNotifier>();

            return services;
        }
    }
}
