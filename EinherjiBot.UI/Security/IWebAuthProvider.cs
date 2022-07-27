using TehGM.EinherjiBot.Security;
using TehGM.EinherjiBot.Security.API;

namespace TehGM.EinherjiBot.UI.Security
{
    public interface IWebAuthProvider : IAuthProvider
    {
        public DateTime Expiration { get; }
        public string Token { get; }

        Task LoginAsync(LoginResponse response, CancellationToken cancellationToken = default);
        Task LogoutAsync(CancellationToken cancellationToken = default);
    }
}
