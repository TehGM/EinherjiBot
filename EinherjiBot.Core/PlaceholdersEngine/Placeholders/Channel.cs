namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder("Channel", PlaceholderUsage.Any)]
    public class ChannelPlaceholder
    {
        [PlaceholderProperty("ID", IsRequired = true, IDType = IDType.User)]
        public ulong ChannelID { get; init; }
        [PlaceholderProperty("Mode")]
        public ChannelDisplayMode DisplayMode { get; init; } = ChannelDisplayMode.Mention;
    }
}
