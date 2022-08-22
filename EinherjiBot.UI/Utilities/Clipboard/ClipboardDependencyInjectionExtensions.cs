using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.UI;
using TehGM.EinherjiBot.UI.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ClipboardDependencyInjectionExtensions
    {
        public static IServiceCollection AddClipboard(this IServiceCollection services)
        {
            services.TryAddTransient<IClipboard, JsInteropClipboard>();

            return services;
        }
    }
}
