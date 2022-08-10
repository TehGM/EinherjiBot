namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder("CurrentChannel", PlaceholderUsage.GuildMessageContext)]
    public class CurrentChannelPlaceholder
    {
        [PlaceholderProperty("Mode")]
        public ChannelDisplayMode DisplayMode { get; init; } = ChannelDisplayMode.Mention;
    }
}
