namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder("CurrentUser", PlaceholderUsage.AnyMessageContext)]
    [DisplayName("Current User")]
    public class CurrentUserPlaceholder
    {
        [PlaceholderProperty("Mode")]
        [DisplayName("Display Mode")]
        public GuildUserDisplayMode DisplayMode { get; init; } = GuildUserDisplayMode.Nickname;
        [PlaceholderProperty("FallbackMode")]
        [DisplayName("Fallback Display Mode")]
        public UserDisplayMode FallbackDisplayMode { get; init; } = UserDisplayMode.Username;
    }
}
