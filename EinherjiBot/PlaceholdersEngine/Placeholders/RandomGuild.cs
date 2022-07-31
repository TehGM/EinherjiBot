using Discord;
using TehGM.Utilities.Randomization;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder("{{RandomGuild}}")]
    public class RandomGuild : IPlaceholder
    {
        private readonly IDiscordClient _client;
        private readonly IRandomizer _randomizer;

        public RandomGuild(IDiscordClient client, IRandomizer randomizer)
        {
            this._client = client;
            this._randomizer = randomizer;
        }

        public async Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
        {
            IEnumerable<IGuild> guilds = await this._client.GetGuildsAsync(CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            IGuild randomGuild = this._randomizer.GetRandomValue(guilds);
            return randomGuild.Name;
        }
    }
}
