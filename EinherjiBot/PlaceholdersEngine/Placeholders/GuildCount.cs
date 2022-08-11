using Discord;
using TehGM.EinherjiBot.Security.Policies;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [AuthorizeBotOrAdmin]
    public class GuildCountPlaceholderHandler : PlaceholderHandler<GuildCountPlaceholder>
    {
        private readonly IDiscordClient _client;

        public GuildCountPlaceholderHandler(IDiscordClient client, IAuthContext auth)
        {
            this._client = client;
        }

        protected override async Task<string> GetReplacementAsync(GuildCountPlaceholder placeholder, CancellationToken cancellationToken = default)
        {
            IEnumerable<IGuild> guilds = await this._client.GetGuildsAsync(CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            return guilds.Count().ToString();
        }
    }
}
