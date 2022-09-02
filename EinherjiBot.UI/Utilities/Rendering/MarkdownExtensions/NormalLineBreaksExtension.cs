using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace TehGM.EinherjiBot.UI.Utilities.Rendering.MarkdownExtensions
{
    // based on following issue: https://github.com/xoofx/markdig/issues/85
    public class NormalLineBreaksExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            renderer.ObjectRenderers.RemoveAll(x => x is ParagraphRenderer);
            renderer.ObjectRenderers.Add(new NormalLineBreaksParagraphRenderer());
        }
    }

    public class NormalLineBreaksParagraphRenderer : ParagraphRenderer
    {
        protected override void Write(HtmlRenderer renderer, ParagraphBlock obj)
        {
            if (obj.Parent is not MarkdownDocument)
            {
                base.Write(renderer, obj);
                return;
            }

            if (!renderer.IsFirstInContainer)
                renderer.EnsureLine();

            renderer.WriteLeafInline(obj);

            if (!renderer.IsLastInContainer)
            {
                renderer.WriteLine("<br />");
                renderer.WriteLine("<br />");
            }
            else
                renderer.EnsureLine();
        }
    }
}
