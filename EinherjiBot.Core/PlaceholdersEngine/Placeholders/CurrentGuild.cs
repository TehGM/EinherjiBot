namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder("CurrentGuild", PlaceholderUsage.GuildMessageContext | PlaceholderUsage.GuildEvent)]
    [DisplayName("Current Guild")]
    [Description("Is replaced with name of guild the message was sent in.")]
    public class CurrentGuildPlaceholder { }
}
