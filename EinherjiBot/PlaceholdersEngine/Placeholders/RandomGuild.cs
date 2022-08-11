using Discord;
using TehGM.EinherjiBot.API;
using TehGM.Utilities.Randomization;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    public class RandomGuildPlaceholderHandler : PlaceholderHandler<RandomGuildPlaceholder>
    {
        private readonly IDiscordClient _client;
        private readonly IRandomizer _randomizer;
        private readonly IAuthContext _auth;

        public RandomGuildPlaceholderHandler(IDiscordClient client, IRandomizer randomizer, IAuthContext auth)
        {
            this._client = client;
            this._randomizer = randomizer;
            this._auth = auth;
        }

        protected override async Task<string> GetReplacementAsync(RandomGuildPlaceholder placeholder, CancellationToken cancellationToken = default)
        {
            if (!this._auth.IsAdmin() && !this._auth.IsEinherji())
                throw new AccessForbiddenException("You're not authorized to use one or more placeholders");

            IEnumerable<IGuild> guilds = await this._client.GetGuildsAsync(CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            IGuild randomGuild = this._randomizer.GetRandomValue(guilds);
            return randomGuild.Name;
        }
    }
}
