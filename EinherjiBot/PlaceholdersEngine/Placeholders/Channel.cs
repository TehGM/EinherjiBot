using Discord;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder($"{{{{Channel:(\\d{{1,20}})(?::({_modeMention}|{_modeName}))?}}}}")]
    internal class Channel : IPlaceholder
    {
        private const string _modeMention = "Mention";
        private const string _modeName = "Name";
        private const string _defaultMode = _modeMention;

        private readonly IDiscordClient _client;

        public Channel(IDiscordClient client)
        {
            this._client = client;
        }

        public async Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
        {
            if (!placeholder.Groups[1].Success)
                throw new ArgumentException($"Placeholder requires a valid channel ID to be provided");
            if (!ulong.TryParse(placeholder.Groups[1].Value, out ulong id))
                throw new ArgumentException($"Placeholder: {placeholder.Groups[1].Value} is not a valid channel ID");

            IChannel channel = await this._client.GetChannelAsync(id, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            if (channel == null)
                throw new InvalidOperationException($"Discord channel with ID {id} not found");

            string mode = _defaultMode;
            if (placeholder.Groups[2].Success)
                mode = placeholder.Groups[2].Value;

            if (mode == _modeMention)
                return MentionUtils.MentionChannel(channel.Id);
            else
                return channel.Name;
        }
    }
}
