namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder("Channel", PlaceholderUsage.Any)]
    public class ChannelPlaceholder
    {
        [PlaceholderProperty("ID", IsRequired = true, IDType = IDType.Channel)]
        [DisplayName("Channel ID")]
        public ulong ChannelID { get; init; }
        [PlaceholderProperty("Mode")]
        [DisplayName("Display Mode")]
        public ChannelDisplayMode DisplayMode { get; init; } = ChannelDisplayMode.Mention;
    }
}
