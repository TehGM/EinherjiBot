using Discord;
using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder("Channel", PlaceholderUsage.Any)]
    [Description("Is replaced with name/mention of a specific channel.")]
    public class ChannelPlaceholder
    {
        [PlaceholderProperty("ID", IsRequired = true, IDType = IDType.Channel)]
        [DisplayName("Channel ID")]
        [Description("ID of the channel. <i>Click on the purple button to open the channel picker!</i>")]
        public ulong ChannelID { get; init; }
        [PlaceholderProperty("Mode")]
        [DisplayName("Display Mode")]
        [Description("Determines how the channel will be displayed.")]
        public ChannelDisplayMode DisplayMode { get; init; } = ChannelDisplayMode.Mention;

        public class ChannelPlaceholderHandler : PlaceholderHandler<ChannelPlaceholder>
        {
            private readonly IDiscordEntityInfoProvider _provider;

            public ChannelPlaceholderHandler(IDiscordEntityInfoProvider provider)
            {
                this._provider = provider;
            }

            protected override async Task<string> GetReplacementAsync(ChannelPlaceholder placeholder, CancellationToken cancellationToken = default)
            {
                ChannelInfoResponse channel = await this._provider.GetChannelInfoAsync(placeholder.ChannelID, cancellationToken).ConfigureAwait(false);
                if (channel == null)
                    throw new PlaceholderConvertException($"Discord channel with ID {placeholder.ChannelID} not found, or is not visible by {EinherjiInfo.Name}");

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
}
