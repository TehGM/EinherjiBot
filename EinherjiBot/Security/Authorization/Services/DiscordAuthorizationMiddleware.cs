using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Net;

namespace TehGM.EinherjiBot.Security.Authorization.Services
{
    public class DiscordAuthorizationMiddleware : IMiddleware
    {
        private readonly IDiscordAuthContext _auth;
        private readonly IDiscordAuthorizationService _service;

        public DiscordAuthorizationMiddleware(IDiscordAuthContext auth, IDiscordAuthorizationService service)
        {
            this._auth = auth;
            this._service = service;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            Endpoint endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
            if (endpoint == null)
            {
                await next.Invoke(context).ConfigureAwait(false);
                return;
            }

            AllowAnonymousAttribute allowAnonymous = endpoint.Metadata.GetMetadata<AllowAnonymousAttribute>();
            if (allowAnonymous != null)
            {
                await next.Invoke(context).ConfigureAwait(false);
                return;
            }

            if (!this._auth.IsLoggedIn())
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Not authenticated", context.RequestAborted).ConfigureAwait(false);
                return;
            }

            if (this._auth.IsBanned)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Banned", context.RequestAborted).ConfigureAwait(false);
                return;
            }

            IEnumerable<IDiscordAuthorizationPolicyAttribute> policies = endpoint.Metadata.GetOrderedMetadata<IDiscordAuthorizationPolicyAttribute>();
            if (!policies.Any())
            {
                await next.Invoke(context).ConfigureAwait(false);
                return;
            }

            DiscordAuthorizationResult result = await this._service.AuthorizeAsync(policies.Select(p => p.PolicyType), context.RequestAborted).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsync(result.Reason ?? "Unauthorized", context.RequestAborted).ConfigureAwait(false);
                return;
            }

            await next.Invoke(context).ConfigureAwait(false);
        }
    }
}
