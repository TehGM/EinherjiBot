using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.UI.Utilities.Markdown;
using TehGM.EinherjiBot.UI.Utilities.Markdown.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MarkdownCollectionExtensions
    {
        public static IServiceCollection AddMarkdown(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<IMarkdownRenderer, MarkdownRenderer>();

            return services;
        }
    }
}
