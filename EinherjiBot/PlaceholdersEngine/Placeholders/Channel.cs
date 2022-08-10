using Discord;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    internal class ChannelPlaceholderHandler : PlaceholderHandler<ChannelPlaceholder>
    {
        private readonly IDiscordClient _client;

        public ChannelPlaceholderHandler(IDiscordClient client)
        {
            this._client = client;
        }

        protected override async Task<string> GetReplacementAsync(ChannelPlaceholder placeholder, CancellationToken cancellationToken = default)
        {
            IChannel channel = await this._client.GetChannelAsync(placeholder.ChannelID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            if (channel == null)
                throw new InvalidOperationException($"Discord channel with ID {placeholder.ChannelID} not found");


            switch (placeholder.DisplayMode)
            {
                case ChannelDisplayMode.Mention:
                    return MentionUtils.MentionChannel(placeholder.ChannelID);
                case ChannelDisplayMode.Name:
                    return channel.Name;
                default:
                    throw new ArgumentException($"Unsupported display mode {placeholder.DisplayMode}", nameof(placeholder.DisplayMode));
            }
        }
    }
}
