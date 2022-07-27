namespace TehGM.EinherjiBot.Security.API
{
    public interface IDiscordAuthHttpClient : IDiscordHttpClient
    {
        Task<DiscordAccessTokenResponse> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<DiscordAccessTokenResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
}
