namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder("CurrentUser", PlaceholderUsage.AnyMessageContext)]
    public class CurrentUserPlaceholder
    {
        [PlaceholderProperty("ID", IsRequired = true, IDType = IDType.User)]
        public ulong UserID { get; init; }
        [PlaceholderProperty("Mode")]
        public GuildUserDisplayMode DisplayMode { get; init; } = GuildUserDisplayMode.Nickname;
        [PlaceholderProperty("FallbackMode")]
        public UserDisplayMode FallbackDisplayMode { get; init; } = UserDisplayMode.Username;
    }
}
