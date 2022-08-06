using TehGM.Utilities.Randomization;
using TehGM.Utilities.Randomization.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    // src: https://tehgm.net/blog/tehgm-csharp-utilities-v0-1-0/#irandomizer-and-irandomizerprovider
    public static class RandomizerServiceCollectionExtensions
    {
        public static IServiceCollection AddRandomizer(this IServiceCollection services)
        {
            services.TryAddSingleton<IRandomizerProvider, RandomizerProvider>();
            services.TryAddTransient<IRandomizer>(provider => provider.GetRequiredService<IRandomizerProvider>().GetSharedRandomizer());

            return services;
        }
    }
}