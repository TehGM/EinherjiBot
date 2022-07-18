using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.Patchbot;
using TehGM.EinherjiBot.Patchbot.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PatchbotDependencyInjectionExtensions
    {
        public static IServiceCollection AddPatchbot(this IServiceCollection services, Action<PatchbotOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure != null)
                services.Configure(configure);

            services.AddDiscordClient();
            services.AddMongoDB();
            services.AddEntityCaching();
            services.TryAddSingleton<IPatchbotGamesStore, MongoPatchbotGameStore>();

            return services;
        }
    }
}
