using Discord;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [OldPlaceholder("{{GuildCount}}")]
    public class GuildCount : IPlaceholder
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
