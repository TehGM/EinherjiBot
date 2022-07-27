using Microsoft.AspNetCore.Http;

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
                this._provider.User = auth;
                context.Features.Set(auth);
            }
            await next(context).ConfigureAwait(false);
        }
    }
}
