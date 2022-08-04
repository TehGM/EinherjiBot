using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.Security;
using TehGM.EinherjiBot.Security.API;
using TehGM.EinherjiBot.Security.Authorization;
using TehGM.EinherjiBot.Security.Authorization.Services;
using TehGM.EinherjiBot.UI.API;
using TehGM.EinherjiBot.UI.API.Services;
using TehGM.EinherjiBot.UI.Security;
using TehGM.EinherjiBot.UI.Security.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthFrontend(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            services.TryAddTransient<IDiscordAuthorizationService, DiscordAuthorizationService>();
            services.TryAddScoped<IWebAuthProvider, WebAuthenticationStateProvider>();
            services.TryAddScoped<IAuthProvider>(s => s.GetRequiredService<IWebAuthProvider>());
            services.TryAddScoped<AuthenticationStateProvider>(s => (AuthenticationStateProvider)s.GetRequiredService<IWebAuthProvider>());
            services.TryAddTransient<IUserInfoService, WebUserInfoService>();

            services.TryAddTransient<IRefreshTokenProvider, WebRefreshTokenProvider>();

            services.TryAddTransient<VersionCheckHttpHandler>();

            services.AddHttpClient<IApiClient, ApiHttpClient>()
                .AddHttpMessageHandler<VersionCheckHttpHandler>();
            services.AddHttpClient<IAuthService, WebAuthService>()
                .AddHttpMessageHandler<VersionCheckHttpHandler>();

            return services;
        }

        public static IServiceCollection AddAuthShared(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddTransient<IDiscordLoginRedirect, DiscordLoginRedirect>();

            return services;
        }
    }
}
