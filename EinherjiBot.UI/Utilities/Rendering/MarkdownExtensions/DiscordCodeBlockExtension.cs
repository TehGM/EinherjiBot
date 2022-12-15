using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace TehGM.EinherjiBot.UI.Utilities.Rendering.MarkdownExtensions
{
    // based on following issue: https://github.com/xoofx/markdig/issues/85
    public class DiscordCodeBlockExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            pipeline.BlockParsers.RemoveAll(x => x is FencedBlockParserBase<FencedCodeBlock>);
            pipeline.BlockParsers.Add(new DiscordFencedCodeBlockParser());
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            renderer.ObjectRenderers.RemoveAll(x => x is HtmlObjectRenderer<CodeBlock>);
            renderer.ObjectRenderers.Add(new DiscordFencedCodeBlockRenderer());
        }
    }

    public class DiscordFencedCodeBlockRenderer : HtmlObjectRenderer<FencedCodeBlock>
    {
        protected override void Write(HtmlRenderer renderer, FencedCodeBlock obj)
        {
            renderer.EnsureLine();

            if (renderer.EnableHtmlForBlock)
            {
                renderer.Write("<pre class=\"discord-code-block\"><code class=\"discord-code\">");
            }

            renderer.WriteLeafRawLines(obj, true, true);

            if (renderer.EnableHtmlForBlock)
            {
                renderer.WriteLine("</code></pre>");
            }

            if (obj.TriviaAfter.Text != null)
            {
                if (obj.TriviaAfter.CurrentChar == '\n' || obj.NewLine != NewLine.None)
                    renderer.WriteLine("<br/>");
                else
                    renderer.Write(obj.TriviaAfter);
            }

            renderer.EnsureLine();
        }
    }

    // TODO: make this actually work like Discord, currently it doesn't
    // unlike commonmark, Discord allows for non-whitespace chars after the closing ```
    // we need to override the way parser determines if it should close
    // let's copy Markdig's parser code again
    public class DiscordFencedCodeBlockParser : FencedCodeBlockParser
    {
        public DiscordFencedCodeBlockParser() : base()
        {
            base.OpeningCharacters = new[] { '`' };
        }

        public override BlockState TryContinue(BlockProcessor processor, Block block)
        {
            IFencedBlock fence = (IFencedBlock)block;
            int openingCount = fence.OpeningFencedCharCount;

            StringSlice line = processor.Line;
            int sourcePosition = processor.Start;
            int closingCount = line.CountAndSkipChar(fence.FencedChar);
            int diff = openingCount - closingCount;

            int lastFenceCharPosition = processor.Start + closingCount;
            int startBeforeTrim = line.Start;
            int endBeforeTrim = line.End;
            line.TrimEnd();

            // here the orignal code checks for EOL, and if it has other chars after ''', it'll not run the if's code
            // we want it to run regardless of that, and just store whatever is after the fences so we can render it after
            if (diff <= 0 && !processor.IsCodeIndent)
            {
                block.UpdateSpanEnd(startBeforeTrim - 1);

                fence.ClosingFencedCharCount = closingCount;

                if (processor.TrackTrivia)
                {
                    fence.NewLine = processor.Line.NewLine;
                    fence.TriviaBeforeClosingFence = processor.UseTrivia(sourcePosition - 1);
                    fence.TriviaAfter = new StringSlice(processor.Line.Text, lastFenceCharPosition, endBeforeTrim);
                }

                // Don't keep the last line
                return BlockState.BreakDiscard;
            }

            // Reset the indentation to the column before the indent
            processor.GoToColumn(processor.ColumnBeforeIndent);

            return BlockState.Continue;
        }
    }
}
