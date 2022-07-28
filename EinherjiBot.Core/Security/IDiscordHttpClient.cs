using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.Security
{
    public interface IDiscordHttpClient
    {
        Task<UserInfoResponse> GetCurrentUserAsync(string bearerToken, CancellationToken cancellationToken = default);
    }
}
