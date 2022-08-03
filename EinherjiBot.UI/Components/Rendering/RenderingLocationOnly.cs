using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace TehGM.EinherjiBot.UI.Components.Rendering
{
    public class RenderingLocationOnly : ComponentBase
    {
        [Inject]
        private IRenderLocation RenderLocation { get; init; }

        [Parameter]
        public RenderLocation Location { get; set; }

        [Parameter, EditorRequired]
        public RenderFragment ChildContent { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (this.RenderLocation.RenderLocation != this.Location)
                return;

            builder.AddContent(1, this.ChildContent);
        }
    }

    public class ClientOnly : RenderingLocationOnly
    {
        public ClientOnly()
        {
            base.Location = RenderLocation.Client;
        }
    }

    public class ServerOnly : RenderingLocationOnly
    {
        public ServerOnly()
        {
            base.Location = RenderLocation.Server;
        }
    }
}
