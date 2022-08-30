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
    }
}
