using Microsoft.AspNetCore.Http;
using System.Net;

namespace TehGM.EinherjiBot.Security.API.Services
{
    public class AuthContextMiddleware : IMiddleware
    {
        private readonly IDiscordAuthProvider _provider;

        public AuthContextMiddleware(IDiscordAuthProvider provider)
        {
            this._provider = provider;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.User?.Identity?.IsAuthenticated == true && ulong.TryParse(context.User?.Identity?.Name, out ulong id))
            {
                IDiscordAuthContext auth = await this._provider.GetAsync(id, null, context.RequestAborted).ConfigureAwait(false);
                if (auth.IsBanned)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync("Banned", context.RequestAborted).ConfigureAwait(false);
                    return;
                }

                this._provider.User = auth;
                context.Features.Set<IDiscordAuthContext>(auth);
                context.Features.Set<IAuthContext>(auth);
            }
            await next(context).ConfigureAwait(false);
        }
    }
}
