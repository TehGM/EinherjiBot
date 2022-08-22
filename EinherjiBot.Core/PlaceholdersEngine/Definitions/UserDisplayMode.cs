namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    public enum UserDisplayMode
    {
        [Description("Will mention the user (<i>@User</i>),")]
        Mention = 0,
        [Description("Will use user's name as normal text (<i>User</i>).")]
        Username = 1,
        [DisplayName("Username with Discriminator")]
        [Description("Will use user's name and discriminator as normal text (<i>User#1234</i>).")]
        UsernameWithDiscriminator = 2
    }
}
