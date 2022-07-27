using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using TehGM.EinherjiBot.Security;
using TehGM.EinherjiBot.Security.API;

namespace TehGM.EinherjiBot.UI.Security.Services
{
    public class EinherjiAuthenticationStateProvider : AuthenticationStateProvider, IWebAuthProvider
    {
        public DateTime Expiration { get; private set; }
        public IAuthContext User { get; private set; } = WebAuthContext.None;
        public string Token { get; private set; }
        private ClaimsPrincipal _principal;

        public bool IsLoggedIn => this.User != null && !this.User.Equals(WebAuthContext.None);

        public void Login(LoginResponse response)
        {
            this.User = WebAuthContext.FromLoginResponse(response);
            this.Token = response.Token;
            this.Expiration = DateTime.UtcNow.AddSeconds(response.TokenExpirationSeconds);
            this._principal = GeneratePrincipal(this.User);
            base.NotifyAuthenticationStateChanged(Task.FromResult(this.GetState()));
        }

        public void Logout()
        {
            this.User = WebAuthContext.None;
            this.Token = null;
            this.Expiration = DateTime.UtcNow;
            this._principal = null;
            base.NotifyAuthenticationStateChanged(Task.FromResult(this.GetState()));
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(this.GetState());

        private static ClaimsPrincipal GeneratePrincipal(IAuthContext user)
        {
            ClaimsIdentity identity = new ClaimsIdentity("jwt", ClaimNames.UserID, ClaimNames.Roles);
            identity.AddClaim(new Claim(ClaimNames.UserID, user.ID.ToString()));
            foreach (string role in user.BotRoles)
                identity.AddClaim(new Claim(ClaimNames.Roles, role));
            return new ClaimsPrincipal(identity);
        }

        private AuthenticationState GetState()
        {
            if (this.IsLoggedIn)
                return new AuthenticationState(this._principal);
            else
                return new AuthenticationState(new ClaimsPrincipal());
        }
    }
}
