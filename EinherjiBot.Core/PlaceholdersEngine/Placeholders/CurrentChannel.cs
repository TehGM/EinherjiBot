using Discord;
using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder("CurrentChannel", PlaceholderUsage.GuildMessageContext | PlaceholderUsage.ChannelEvent)]
    [DisplayName("Current Channel")]
    [Description("Is replaced with name/mention of channel the message was sent in.")]
    public class CurrentChannelPlaceholder
    {
        [PlaceholderProperty("Mode")]
        [DisplayName("Display Mode")]
        [Description("Determines how the channel will be displayed.")]
        public ChannelDisplayMode DisplayMode { get; init; } = ChannelDisplayMode.Mention;

        public class CurrentChannelPlaceholderHandler : PlaceholderHandler<CurrentChannelPlaceholder>
        {
            private readonly PlaceholderConvertContext _context;
            private readonly IDiscordEntityInfoProvider _provider;

            public CurrentChannelPlaceholderHandler(PlaceholderConvertContext context, IDiscordEntityInfoProvider provider)
            {
                this._context = context;
                this._provider = provider;
            }

            protected override async Task<string> GetReplacementAsync(CurrentChannelPlaceholder placeholder, CancellationToken cancellationToken = default)
            {
                if (this._context.CurrentGuildID == null)
                    throw new PlaceholderContextException($"{nameof(CurrentChannelPlaceholder)} is not usable in this context.");

                ChannelInfoResponse channel = await this._provider.GetChannelInfoAsync(this._context.CurrentChannelID.Value, cancellationToken).ConfigureAwait(false);
                switch (placeholder.DisplayMode)
                {
                    case ChannelDisplayMode.Mention:
                        return MentionUtils.MentionChannel(channel.ID);
                    case ChannelDisplayMode.Name:
                        return channel.Name;
                    default:
                        throw new ArgumentException($"Unsupported display mode {placeholder.DisplayMode}", nameof(placeholder.DisplayMode));
                }
            }
        }
    }
}
