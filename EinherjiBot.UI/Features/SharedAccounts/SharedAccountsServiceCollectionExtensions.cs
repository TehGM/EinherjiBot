using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.SharedAccounts;
using TehGM.EinherjiBot.SharedAccounts.API;
using TehGM.EinherjiBot.UI.SharedAccounts;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SharedAccountsServiceCollectionExtensions
    {
        public static IServiceCollection AddSharedAccountsFrontend(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddTransient<ISharedAccountsService, WebSharedAccountsService>();
            services.TryAddScoped<ISharedAccountImageProvider, WebSharedAccountImageProvider>();

            return services;
        }
    }
}
