namespace TehGM.EinherjiBot.UI.Security
{
    public interface IRefreshTokenProvider
    {
        ValueTask<string> GetAsync(CancellationToken cancellationToken = default);
        ValueTask SetAsync(string token, CancellationToken cancellationToken = default);
    }
}
