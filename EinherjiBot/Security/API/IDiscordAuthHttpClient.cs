﻿namespace TehGM.EinherjiBot.Security.API
{
    public interface IDiscordAuthHttpClient : IDiscordHttpClient
    {
        Task<DiscordAccessTokenResponse> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default);
    }
}
