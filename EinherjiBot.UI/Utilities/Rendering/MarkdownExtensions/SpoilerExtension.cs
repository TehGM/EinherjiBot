using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax.Inlines;

namespace TehGM.EinherjiBot.UI.Utilities.Rendering.MarkdownExtensions
{
    public class SpoilerExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            pipeline.InlineParsers.AddIfNotAlready<SpoilerInlineParser>();
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            renderer.ObjectRenderers.AddIfNotAlready<SpoilerInlineRenderer>();
        }
    }

    public class SpoilerInline : LeafInline
    {
        public StringSlice Content { get; set; }
    }

    public class SpoilerInlineRenderer : HtmlObjectRenderer<SpoilerInline>
    {
        protected override void Write(HtmlRenderer renderer, SpoilerInline obj)
        {
            if (!renderer.EnableHtmlForInline)
            {
                renderer.Write("||").Write(obj.Content).Write("||");
                return;
            }
            ReadOnlySpan<char> chars = obj.Content.AsSpan();
            string content = new string(chars.ToArray()).Replace(" ", "&nbsp;");
            renderer.Write("<span class=\"discord-spoiler hidden\"><span class=\"discord-spoiler-text\">").Write(content).Write("</span></span>");
        }
    }

    public class SpoilerInlineParser : InlineParser
    {
        private const char _eol = '\0';

        public SpoilerInlineParser()
        {
            base.OpeningCharacters = new[] { '|' };
        }

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            // this will check if | is double, ie ||, but not triple, ie |||
            slice.NextChar();
            if (!base.OpeningCharacters.Contains(slice.CurrentChar))
                return false;

            slice.NextChar();
            int start = slice.Start;
            int end = slice.Start;

            while (true)
            {
                slice.NextChar();

                char ahead = slice.PeekCharExtra(1);
                if (slice.CurrentChar == _eol || ahead == _eol)
                    return false;
                if (base.OpeningCharacters.Contains(slice.CurrentChar) && base.OpeningCharacters.Contains(ahead))
                    break;

                end = slice.Start;
            }

            // skip the end tag
            slice.SkipChar();
            slice.SkipChar();

            int inlineStart = processor.GetSourcePosition(slice.Start, out int line, out int column);
            processor.Inline = new SpoilerInline()
            {
                Span = { Start = inlineStart, End = inlineStart + 2 + (end - start) },
                Line = line,
                Column = column,
                Content = new StringSlice(slice.Text, start, end)
            };
            return true;
        }
    }
}
