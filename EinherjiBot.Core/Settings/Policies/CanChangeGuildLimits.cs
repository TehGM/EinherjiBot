using TehGM.EinherjiBot.Security;

namespace TehGM.EinherjiBot.Settings.Policies
{
    public class CanChangeGuildLimits : IBotAuthorizationPolicy<IGuildSettings>, IBotAuthorizationPolicy
    {
        private readonly IAuthProvider _auth;

        public CanChangeGuildLimits(IAuthProvider auth)
        {
            this._auth = auth;
        }

        public Task<BotAuthorizationResult> EvaluateAsync(IGuildSettings resource, CancellationToken cancellationToken = default)
        {
            if (this._auth.User.IsAdmin())
                return Task.FromResult(BotAuthorizationResult.Success);

            return Task.FromResult(BotAuthorizationResult.Fail($"You have no permission to change limits of guild {resource.GuildID}."));
        }

        public Task<BotAuthorizationResult> EvaluateAsync(CancellationToken cancellationToken = default)
        {
            if (this._auth.User.IsAdmin())
                return Task.FromResult(BotAuthorizationResult.Success);

            return Task.FromResult(BotAuthorizationResult.Fail($"You have no permission to change guild limits."));
        }
    }
}
