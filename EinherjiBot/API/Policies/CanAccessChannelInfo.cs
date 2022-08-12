using Discord;

namespace TehGM.EinherjiBot.API.Policies
{
    public class CanAccessChannelInfo : IBotAuthorizationPolicy<IChannel>
    {
        private readonly IAuthProvider _auth;

        public CanAccessChannelInfo(IAuthProvider auth)
        {
            this._auth = auth;
        }

        public async Task<BotAuthorizationResult> EvaluateAsync(IChannel resource, CancellationToken cancellationToken = default)
        {
            if (this._auth.User.IsAdmin())
                return BotAuthorizationResult.Success;

            if (resource is IGuildChannel guildChannel)
            {
                IGuildUser guildUser = await guildChannel.Guild.GetUserAsync(this._auth.User.ID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
                ChannelPermissions perms = guildUser.GetPermissions(guildChannel);
                if (perms.ViewChannel)
                    return BotAuthorizationResult.Success;
            }
            else
            {
                IUser user = await resource.GetUserAsync(this._auth.User.ID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions());
                if (user != null)
                    return BotAuthorizationResult.Success;
            }
            return BotAuthorizationResult.Fail($"You have no permission to access channel {resource.Id}.");
        }
    }
}
