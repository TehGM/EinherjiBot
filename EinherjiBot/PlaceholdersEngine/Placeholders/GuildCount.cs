using Discord;
using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    public class GuildCountPlaceholderHandler : PlaceholderHandler<GuildCountPlaceholder>
    {
        private readonly IDiscordClient _client;
        private readonly IAuthContext _auth;

        public GuildCountPlaceholderHandler(IDiscordClient client, IAuthContext auth)
        {
            this._client = client;
            this._auth = auth;
        }

        protected override async Task<string> GetReplacementAsync(GuildCountPlaceholder placeholder, CancellationToken cancellationToken = default)
        {
            if (!this._auth.IsAdmin() && !this._auth.IsEinherji())
                throw new AccessForbiddenException("You're not authorized to use one or more placeholders");

            IEnumerable<IGuild> guilds = await this._client.GetGuildsAsync(CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            return guilds.Count().ToString();
        }
    }
}
