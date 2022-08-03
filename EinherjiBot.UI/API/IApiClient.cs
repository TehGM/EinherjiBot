﻿using System.Net.Http;

namespace TehGM.EinherjiBot.UI.API
{
    public interface IApiClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, object data, CancellationToken cancellationToken = default);
    }
}
