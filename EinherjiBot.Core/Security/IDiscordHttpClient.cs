using TehGM.EinherjiBot.Security.API;

namespace TehGM.EinherjiBot.Security
{
    public interface IDiscordHttpClient
    {
        Task<CurrentUserResponse> GetCurrentUserAsync(string bearerToken, CancellationToken cancellationToken = default);
    }
}
