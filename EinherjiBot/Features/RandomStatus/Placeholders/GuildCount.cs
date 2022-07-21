using Discord;

namespace TehGM.EinherjiBot.RandomStatus.Placeholders
{
    [StatusPlaceholder("{{GuildCount}}")]
    public class GuildCount : IStatusPlaceholder
    {
        private readonly IDiscordClient _client;

        public GuildCount(IDiscordClient client)
        {
            this._client = client;
        }

        public async Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
        {
            IEnumerable<IGuild> guilds = await this._client.GetGuildsAsync(CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            return guilds.Count().ToString();
        }
    }
}
