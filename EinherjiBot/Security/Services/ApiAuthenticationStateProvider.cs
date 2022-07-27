using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace TehGM.EinherjiBot.Security.Services
{
    // exists only to avoid exceptions during pre-rendering
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider, IAuthProvider
    {
        public IAuthContext User { get; }

        public ApiAuthenticationStateProvider(IDiscordAuthContext context)
        {
            this.User = context;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (!this.User.IsLoggedIn())
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal()));
            return Task.FromResult(new AuthenticationState(this.User.ToClaimsPrincipal("jwt")));
        }
    }
}
