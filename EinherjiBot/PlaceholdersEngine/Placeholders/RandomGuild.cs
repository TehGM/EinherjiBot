using Discord;
using TehGM.EinherjiBot.Security.Policies;
using TehGM.Utilities.Randomization;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [AuthorizeBotOrAdmin]
    public class RandomGuildPlaceholderHandler : PlaceholderHandler<RandomGuildPlaceholder>
    {
        private readonly IDiscordClient _client;
        private readonly IRandomizer _randomizer;

        public RandomGuildPlaceholderHandler(IDiscordClient client, IRandomizer randomizer)
        {
            this._client = client;
            this._randomizer = randomizer;
        }

        protected override async Task<string> GetReplacementAsync(RandomGuildPlaceholder placeholder, CancellationToken cancellationToken = default)
        {
            IEnumerable<IGuild> guilds = await this._client.GetGuildsAsync(CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            IGuild randomGuild = this._randomizer.GetRandomValue(guilds);
            return randomGuild.Name;
        }
    }
}
