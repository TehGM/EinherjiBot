using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.UI.Components.EntityInfo.Previews
{
    public abstract partial class PreviewableEntitySpan<TEntity> : ComponentBase where TEntity : class, IDiscordEntityInfo
    {
        [Parameter, EditorRequired]
        public TEntity Entity { get; set; }
        [Parameter]
        public bool EnableTooltip { get; set; } = false;
        [Parameter]
        public bool EnablePopover { get; set; } = false;

        [Parameter]
        public bool ShowAsMention { get; set; } = true;

        [Parameter]
        public EventCallback<MouseEventArgs> Clicked { get; set; }
        [Parameter]
        public string SpanCssClass { get; set; } = null;

        protected bool IsPopoverOpen { get; set; } = false;

        protected abstract string GetText();
        protected abstract RenderFragment RenderTooltipPreview();
        protected abstract RenderFragment RenderPopoverPreview();


        protected virtual string BuildCssClass()
        {
            List<string> cssClasses = new List<string>(3);

            if (this.ShowAsMention)
                cssClasses.Add("discord-mention");
            if (this.EnablePopover || this.Clicked.HasDelegate)
                cssClasses.Add("cursor-pointer");
            cssClasses.Add(this.SpanCssClass ?? string.Empty);

            return string.Join(' ', cssClasses);
        }

        protected virtual Task OnClickedAsync(MouseEventArgs e)
        {
            if (this.EnablePopover)
                this.IsPopoverOpen = true;
            return this.Clicked.InvokeAsync(e);
        }
    }
}
