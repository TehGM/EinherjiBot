﻿using Microsoft.AspNetCore.Http;

namespace TehGM.EinherjiBot.Security.API.Services
{
    public class ApiExceptionsMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiExceptionsMiddleware(RequestDelegate next)
        {
            this._next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await this._next(context).ConfigureAwait(false);
            }
            catch (ApiException ex)
            {
                context.Response.Clear();
                context.Response.StatusCode = (int)ex.StatusCode;
                context.Response.ContentType = "text/plain";
                context.Response.Headers.CacheControl = "no-cache,no-store";
                context.Response.Headers.Pragma = "no-cache";
                context.Response.Headers.Expires = "-1";
                context.Response.Headers.ETag = default;
                await context.Response.WriteAsync(ex.Message).ConfigureAwait(false);
            }
        }
    }
}