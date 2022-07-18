using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.Stellaris;
using TehGM.EinherjiBot.Stellaris.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class StellarisDependencyInjectionExtensions
    {
        public static IServiceCollection AddStellaris(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddDiscordClient();
            services.AddMongoDB();
            services.AddEntityCaching();
            services.TryAddSingleton<IStellarisModsStore, MongoStellarisModsStore>();

            return services;
        }
    }
}
