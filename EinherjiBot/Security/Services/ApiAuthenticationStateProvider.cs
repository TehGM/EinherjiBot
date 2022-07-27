using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace TehGM.EinherjiBot.Security.Services
{
    // exists only to avoid exceptions during pre-rendering
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider, IAuthProvider
    {
        public IAuthContext User { get; }
        public bool IsLoggedIn => this.User != null && !this.User.Equals(DiscordSocketAuthContext.None);
        public string Token => throw new NotImplementedException();
        public DateTime Expiration => throw new NotImplementedException();

        public ApiAuthenticationStateProvider(IDiscordAuthContext context)
        {
            this.User = context;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (this.IsLoggedIn)
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal()));
            return Task.FromResult(new AuthenticationState(this.User.ToClaimsPrincipal("jwt")));
        }
    }
}
