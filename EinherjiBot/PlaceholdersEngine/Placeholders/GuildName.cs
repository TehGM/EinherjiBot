using Discord;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder($"{{{{GuildName:(\\d{{1,20}})}}}}")]
    internal class GuildName : IPlaceholder
    {
        private readonly IDiscordClient _client;

        private string _name;

        public GuildName(IDiscordClient client)
        {
            this._client = client;
        }

        public async Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(this._name))
                return this._name;

            if (!placeholder.Groups[1].Success)
                throw new ArgumentException($"Placeholder requires a valid channel ID to be provided");
            if (!ulong.TryParse(placeholder.Groups[1].Value, out ulong id))
                throw new ArgumentException($"Placeholder: {placeholder.Groups[1].Value} is not a valid guild ID");

            IGuild guild = await this._client.GetGuildAsync(id, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            if (guild == null)
                throw new InvalidOperationException($"Discord guild with ID {id} not found");

            this._name = guild.Name;
            return this._name;
        }
    }
}
