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

            // voice channels are a special case
            // for some stupid reason they, unlike any other channel, only return user if he's connected, regardless of access permissions
            if (resource is IVoiceChannel voiceChannel)
            {
                IGuildUser guildUser = await voiceChannel.Guild.GetUserAsync(this._auth.User.ID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
                ChannelPermissions perms = guildUser.GetPermissions(voiceChannel);
                if (perms.ViewChannel)
                    return BotAuthorizationResult.Success;
            }

            IUser user = await resource.GetUserAsync(this._auth.User.ID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions());
            if (user != null)
                return BotAuthorizationResult.Success;
            return BotAuthorizationResult.Fail($"You have no permission to access channel {resource.Id}.");
        }
    }
}
