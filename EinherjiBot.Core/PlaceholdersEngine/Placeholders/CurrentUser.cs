namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder("CurrentUser", PlaceholderUsage.AnyMessageContext | PlaceholderUsage.UserEvent)]
    [DisplayName("Current User")]
    [Description("Is replaced with mention/name of the user that sent the message.")]
    public class CurrentUserPlaceholder
    {
        [PlaceholderProperty("Mode")]
        [DisplayName("Display Mode")]
        [Description("Determines how the user will be displayed.")]
        public GuildUserDisplayMode DisplayMode { get; init; } = GuildUserDisplayMode.Nickname;
        [PlaceholderProperty("FallbackMode")]
        [DisplayName("Fallback Display Mode")]
        [Description("Determines how the user will be displayed if Nickname was not found. Ignored when default display mode isn't set to Nickname.")]
        public UserDisplayMode FallbackDisplayMode { get; init; } = UserDisplayMode.Username;
    }
}
