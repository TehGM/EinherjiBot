using Microsoft.JSInterop;

namespace TehGM.EinherjiBot.UI.Services
{
    public class JsInteropClipboard : IClipboard
    {
        private readonly IJSRuntime _jsRuntime;

        public JsInteropClipboard(IJSRuntime jsRuntime)
        {
            this._jsRuntime = jsRuntime;
        }

        public ValueTask WriteTextAsync(string text, CancellationToken cancellationToken = default)
            => this._jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", cancellationToken, text ?? string.Empty);
    }
}
