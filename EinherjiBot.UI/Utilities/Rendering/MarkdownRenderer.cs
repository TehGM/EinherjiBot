using Ganss.XSS;
using Markdig;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using TehGM.EinherjiBot.UI.Utilities.Rendering.MarkdownExtensions;

namespace TehGM.EinherjiBot.UI.Utilities.Markdown.Services
{
    public class MarkdownRenderer : IMarkdownRenderer
    {
        private readonly IHtmlSanitizer _sanitizer;

        private MarkdownPipeline _discordTextPipeline;
        private MarkdownPipeline _discordEmbedTextPipeline;

        public MarkdownRenderer()
        {
            this._sanitizer = new HtmlSanitizer();
        }

        public string RenderDiscordText(string text)
        {
            MarkdownPipeline pipeline = this.BuildDiscordTextPipeline();
            string result = this.ConvertToHtml(text, pipeline);
            return TrimHtml(result);
        }

        public string RenderDiscordEmbedText(string text)
        {
            MarkdownPipeline pipeline = this.BuildDiscordEmbedTextPipeline();
            string result = this.ConvertToHtml(text, pipeline);
            return TrimHtml(result);
        }

        private string ConvertToHtml(string text, MarkdownPipeline pipeline)
        {
            string result = Markdig.Markdown.ToHtml(text, pipeline);
            return this._sanitizer.Sanitize(result);
        }

        private static string TrimHtml(string text)
        {
            string result = text.TrimEnd('\n');
            //if (result.EndsWith("</p>"))
            //    result = result.Remove(result.Length - "</p>".Length);
            //if (result.StartsWith("<p>"))
            //    result = result.Substring("<p>".Length);
            return result;
        }

        #region Pipeline Builders
        private MarkdownPipeline BuildDiscordTextPipeline()
        {
            if (this._discordTextPipeline == null)
                this._discordTextPipeline = CreateDiscordPipeline().Build();

            return this._discordTextPipeline;
        }

        private MarkdownPipeline BuildDiscordEmbedTextPipeline()
        {
            if (this._discordEmbedTextPipeline == null)
            {
                MarkdownPipelineBuilder builder = CreateDiscordPipeline();
                builder.InlineParsers.Insert(0, new LinkInlineParser() { OpeningCharacters = new[] { '[' } });
                this._discordEmbedTextPipeline = builder.Build();
            }

            return this._discordEmbedTextPipeline;
        }

        private static MarkdownPipelineBuilder CreateDiscordPipeline()
        {
            MarkdownPipelineBuilder builder = new MarkdownPipelineBuilder();

            // not all of markdown features are supported by discord messages, so only add those that are Discord-y enough
            builder.BlockParsers.Clear();
            builder.InlineParsers.Clear();
            builder.BlockParsers.Add(new QuoteBlockParser());
            builder.BlockParsers.Add(new FencedCodeBlockParser() { OpeningCharacters = new[] { '`' } });
            builder.BlockParsers.Add(new IndentedCodeBlockParser());
            builder.BlockParsers.Add(new ParagraphBlockParser());
            builder.InlineParsers.Add(new EscapeInlineParser());
            builder.InlineParsers.Add(new EmphasisInlineParser());
            builder.InlineParsers.Add(new CodeInlineParser());
            builder.InlineParsers.Add(new AutolinkInlineParser() { EnableHtmlParsing = false });
            builder.InlineParsers.Add(new LineBreakInlineParser() { EnableSoftAsHard = true });
            builder.UseEmphasisExtras(Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Strikethrough);
            builder.UseReferralLinks("nofollow");
            builder.EnableTrackTrivia();
            builder.Extensions.AddIfNotAlready<NormalLineBreaksExtension>();

            // TODO: add parsers for other discord tags

            return builder;
        }
        #endregion
    }
}
