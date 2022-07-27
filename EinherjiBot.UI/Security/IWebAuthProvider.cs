using TehGM.EinherjiBot.Security;
using TehGM.EinherjiBot.Security.API;

namespace TehGM.EinherjiBot.UI.Security
{
    public interface IWebAuthProvider
    {
        public DateTime Expiration { get; }
        public IAuthContext User { get; }
        public string Token { get; }
        public bool IsLoggedIn { get; }

        void Login(LoginResponse response);
        void Logout();
    }
}
