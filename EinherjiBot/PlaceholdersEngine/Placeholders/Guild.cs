using Discord;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder($"{{{{Guild:(\\d{{1,20}})}}}}", DisplayName = "{{Guild}}")]
    internal class Guild : IPlaceholder
    {
        private readonly IDiscordClient _client;

        public Guild(IDiscordClient client)
        {
            this._client = client;
        }

        public async Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
        {
            if (!placeholder.Groups[1].Success)
                throw new ArgumentException($"Placeholder requires a valid channel ID to be provided");
            if (!ulong.TryParse(placeholder.Groups[1].Value, out ulong id))
                throw new ArgumentException($"Placeholder: {placeholder.Groups[1].Value} is not a valid guild ID");

            IGuild guild = await this._client.GetGuildAsync(id, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            if (guild == null)
                throw new InvalidOperationException($"Discord guild with ID {id} not found");

            return guild.Name;
        }
    }
}
