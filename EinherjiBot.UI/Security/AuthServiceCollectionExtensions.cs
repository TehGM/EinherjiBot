using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.Security;
using TehGM.EinherjiBot.Security.API;
using TehGM.EinherjiBot.Security.Services;
using TehGM.EinherjiBot.UI.API;
using TehGM.EinherjiBot.UI.API.Handlers;
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
            services.TryAddTransient<IBotAuthorizationService, BotAuthorizationService>();
            services.TryAddScoped<IWebAuthProvider, WebAuthenticationStateProvider>();
            services.TryAddScoped<IAuthProvider>(s => s.GetRequiredService<IWebAuthProvider>());
            services.TryAddScoped<AuthenticationStateProvider>(s => (AuthenticationStateProvider)s.GetRequiredService<IWebAuthProvider>());

            services.TryAddTransient<IRefreshTokenProvider, WebRefreshTokenProvider>();

            services.TryAddTransient<UserAgentHttpHandler>();
            services.TryAddTransient<VersionCheckHttpHandler>();
            services.TryAddTransient<RequestExceptionsHttpHandler>();

            services.AddHttpClient<IApiClient, ApiHttpClient>().AttachHttpHandlers();
            services.AddHttpClient<IAuthService, WebAuthService>().AttachHttpHandlers();

            return services;
        }

        private static IHttpClientBuilder AttachHttpHandlers(this IHttpClientBuilder builder)
        {
            return builder
                .AddHttpMessageHandler<RequestExceptionsHttpHandler>()
                .AddHttpMessageHandler<VersionCheckHttpHandler>()
                .AddHttpMessageHandler<UserAgentHttpHandler>();
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
