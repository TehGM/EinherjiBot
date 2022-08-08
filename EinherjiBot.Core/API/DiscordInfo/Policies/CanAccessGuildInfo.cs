using Discord;
using TehGM.EinherjiBot.Security;

namespace TehGM.EinherjiBot.API.Policies
{
    public class CanAccessGuildInfo : IBotAuthorizationPolicy<GuildInfoResponse>, IBotAuthorizationPolicy<IGuild>
    {
        private readonly IAuthProvider _auth;

        public CanAccessGuildInfo(IAuthProvider auth)
        {
            this._auth = auth;
        }

        public Task<BotAuthorizationResult> EvaluateAsync(GuildInfoResponse resource, CancellationToken cancellationToken = default)
        {
            if (this._auth.User.IsAdmin() || this._auth.User.KnownDiscordGuildIDs?.Contains(resource.ID) == true)
                return Task.FromResult(BotAuthorizationResult.Success);
            return Task.FromResult(BotAuthorizationResult.Fail($"You have no permission to access guild {resource.ID}."));
        }

        public Task<BotAuthorizationResult> EvaluateAsync(IGuild resource, CancellationToken cancellationToken = default)
        {
            if (this._auth.User.IsAdmin() || this._auth.User.KnownDiscordGuildIDs?.Contains(resource.Id) == true)
                return Task.FromResult(BotAuthorizationResult.Success);
            return Task.FromResult(BotAuthorizationResult.Fail($"You have no permission to access guild {resource.Id}."));
        }
    }
}
