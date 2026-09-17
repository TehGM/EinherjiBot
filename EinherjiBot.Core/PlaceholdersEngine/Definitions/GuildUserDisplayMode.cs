namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    public enum GuildUserDisplayMode
    {
        [Description("Will mention the user (<i>@User</i>),")]
        Mention = 0,
        [Description("Will use user's unique username as normal text (<i>user</i>).")]
        Username = 1,
        [DisplayName("Display Name")]
        [Description("Will use user's display name as normal text (<i>User</i>). Username will be used if user has no display name set.")]
        DisplayName = 3,
        [Description("Will use user's guild nickname if available. Fallback display mode will be used otherwise.")]
        Nickname = 4
    }
}
