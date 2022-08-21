namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    public enum UserDisplayMode
    {
        [Description("Will mention the user (@User),")]
        Mention = 0,
        [Description("Will use user's name as normal text (User).")]
        Username = 1,
        [DisplayName("Username with Discriminator")]
        [Description("Will use user's name and discriminator as normal text (User#1234).")]
        UsernameWithDiscriminator = 2
    }
}
