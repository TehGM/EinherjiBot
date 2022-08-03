using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace TehGM.EinherjiBot.UI.Components.Rendering
{
    public class RenderingEnvironmentOnly : ComponentBase
    {
        [Inject]
        private IRenderLocation RenderLocation { get; init; }

        [Parameter]
        public string Environment { get; set; }

        [Parameter, EditorRequired]
        public RenderFragment ChildContent { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (!this.RenderLocation.EnvironmentName.Equals(this.Environment, StringComparison.OrdinalIgnoreCase))
                return;

            builder.AddContent(1, this.ChildContent);
        }
    }

    public class DevelopmentOnly : RenderingEnvironmentOnly
    {
        public DevelopmentOnly()
        {
            base.Environment = "Development";
        }
    }

    public class ProductionOnly : RenderingEnvironmentOnly
    {
        public ProductionOnly()
        {
            base.Environment = "Production";
        }
    }
}
