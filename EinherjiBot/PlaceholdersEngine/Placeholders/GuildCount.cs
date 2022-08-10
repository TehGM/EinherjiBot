using Discord;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    public class GuildCountPlaceholderHandler : PlaceholderHandler<GuildCountPlaceholder>
    {
        private readonly IDiscordClient _client;

        public GuildCountPlaceholderHandler(IDiscordClient client)
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
