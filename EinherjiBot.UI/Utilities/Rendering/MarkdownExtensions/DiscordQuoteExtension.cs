using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace TehGM.EinherjiBot.UI.Utilities.Rendering.MarkdownExtensions
{
    public class DiscordQuoteExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            pipeline.BlockParsers.TryRemove<QuoteBlockParser>();
            pipeline.BlockParsers.AddIfNotAlready<DiscordQuoteBlockParser>();
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            renderer.ObjectRenderers.TryRemove<QuoteBlockRenderer>();
            renderer.ObjectRenderers.AddIfNotAlready<DiscordQuoteBlockRenderer>();
        }
    }

    public class DiscordQuoteBlockRenderer : QuoteBlockRenderer
    {
        protected override void Write(HtmlRenderer renderer, QuoteBlock obj)
        {
            if (!renderer.EnableHtmlForBlock)
            {
                base.Write(renderer, obj);
                return;
            }

            renderer.EnsureLine();
            renderer.WriteLine("<div class=\"discord-quote\">");
            renderer.WriteLine("<div class=\"discord-quote-line\"></div>");
            renderer.WriteLine("<blockquote>");
            bool savedImplicitParagraph = renderer.ImplicitParagraph;
            renderer.ImplicitParagraph = false;
            renderer.WriteChildren(obj);
            renderer.ImplicitParagraph = savedImplicitParagraph;
            renderer.WriteLine("</blockquote>");
            renderer.WriteLine("</div>");
            renderer.EnsureLine();
        }
    }

    // unfortunately seems the only way to make it ignore > without space after (like Discord) is to rewrite parser
    // this is just copied QuoteBlockParser, altered to behave like Discord
    public class DiscordQuoteBlockParser : QuoteBlockParser
    {
        public DiscordQuoteBlockParser() : base() { }

        public override BlockState TryOpen(BlockProcessor processor)
        {
            if (processor.IsCodeIndent)
                return BlockState.None;

            int sourcePosition = processor.Start;

            char quoteChar = processor.CurrentChar;
            int column = processor.Column;
            char nextChar = processor.PeekChar(1);

            if (nextChar != ' ')
                return BlockState.None;

            QuoteBlock quoteBlock = new QuoteBlock(this)
            {
                QuoteChar = quoteChar,
                Column = column,
                Span = new SourceSpan(sourcePosition, processor.Line.End)
            };

            if (processor.TrackTrivia)
            {
                quoteBlock.LinesBefore = processor.LinesBefore;
            }

            processor.NextColumn();
            processor.SkipFirstUnwindSpace = true;

            if (processor.TrackTrivia)
            {
                StringSlice triviaBefore = processor.UseTrivia(sourcePosition - 1);
                StringSlice triviaAfter = StringSlice.Empty;
                bool wasEmptyLine = false;
                if (processor.Line.IsEmptyOrWhitespace())
                {
                    processor.TriviaStart = processor.Start;
                    triviaAfter = processor.UseTrivia(processor.Line.End);
                    wasEmptyLine = true;
                }

                if (!wasEmptyLine)
                {
                    processor.TriviaStart = processor.Start;
                }

                quoteBlock.QuoteLines.Add(new QuoteBlockLine
                {
                    TriviaBefore = triviaBefore,
                    TriviaAfter = triviaAfter,
                    QuoteChar = true,
                    HasSpaceAfterQuoteChar = true,
                    NewLine = processor.Line.NewLine,
                });
            }

            processor.NewBlocks.Push(quoteBlock);
            return BlockState.Continue;
        }

        public override BlockState TryContinue(BlockProcessor processor, Block block)
        {
            if (processor.IsCodeIndent)
            {
                return BlockState.None;
            }

            QuoteBlock quote = (QuoteBlock)block;
            int sourcePosition = processor.Start;

            char currentChar = processor.CurrentChar;
            char nextChar = processor.PeekChar(1);
            if (currentChar != quote.QuoteChar || nextChar != ' ')
            {
                if (processor.TrackTrivia)
                {
                    quote.QuoteLines.Add(new QuoteBlockLine
                    {
                        QuoteChar = false,
                        NewLine = processor.Line.NewLine,
                    });
                }
                processor.Close(block);
                return BlockState.None;
            }

            processor.NextChar(); // Skip quote marker char
            processor.NextColumn();
            processor.SkipFirstUnwindSpace = true;

            if (processor.TrackTrivia)
            {
                StringSlice triviaSpaceBefore = processor.UseTrivia(sourcePosition - 1);
                StringSlice triviaAfter = StringSlice.Empty;
                bool wasEmptyLine = false;
                if (processor.Line.IsEmptyOrWhitespace())
                {
                    processor.TriviaStart = processor.Start;
                    triviaAfter = processor.UseTrivia(processor.Line.End);
                    wasEmptyLine = true;
                }
                quote.QuoteLines.Add(new QuoteBlockLine
                {
                    QuoteChar = true,
                    HasSpaceAfterQuoteChar = true,
                    TriviaBefore = triviaSpaceBefore,
                    TriviaAfter = triviaAfter,
                    NewLine = processor.Line.NewLine
                });

                if (!wasEmptyLine)
                {
                    processor.TriviaStart = processor.Start;
                }
            }

            block.UpdateSpanEnd(processor.Line.End);
            return BlockState.Continue;
        }
    }
}
