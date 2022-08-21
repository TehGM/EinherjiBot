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
    }
}
