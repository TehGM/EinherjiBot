using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Net;

namespace TehGM.EinherjiBot.API.Services
{
    public class VersionCheckMiddleware
    {
        private readonly RequestDelegate _next;

        public VersionCheckMiddleware(RequestDelegate next)
        {
            this._next = next;
        }

        public Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(CustomHeaders.ClientVersion, out StringValues values) ||
                values == StringValues.Empty || values.ToString() != EinherjiInfo.WebVersion)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Headers.Add(CustomHeaders.ExpectedClientVersion, EinherjiInfo.WebVersion);
                return context.Response.WriteAsync("Wrong client version", context.RequestAborted);
            }

            return this._next(context);
        }
    }
}
