namespace TehGM.EinherjiBot.UI.Utilities.Markdown
{
    public interface IMarkdownRenderer
    {
        string RenderDiscordText(string text);
        string RenderDiscordEmbedText(string text);
    }
}
