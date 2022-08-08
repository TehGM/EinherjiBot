using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.Security;
using TehGM.EinherjiBot.Security.API;

namespace TehGM.EinherjiBot.UI.Security
{
    public interface IWebAuthProvider : IAuthProvider
    {
        public DateTime Expiration { get; }
        public string Token { get; }
        public IEnumerable<OAuthGuildInfoResponse> Guilds { get; }
        public IEnumerable<string> UserFeatures { get; }

        Task LoginAsync(LoginResponse response, CancellationToken cancellationToken = default);
        Task LogoutAsync(CancellationToken cancellationToken = default);
    }
}
