using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.Intel;
using TehGM.EinherjiBot.Intel.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class UserIntelServiceCollectionExtensions
    {
        public static IServiceCollection AddUserIntel(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddDiscordClient();
            services.AddMongoDB();
            services.AddEntityCaching();
            services.AddLocking();
            services.TryAddSingleton<IUserIntelStore, MongoUserIntelStore>();
            services.TryAddScoped<IUserIntelProvider, UserIntelProvider>();
            services.TryAddTransient<IIntelEmbedBuilder, IntelEmbedBuilder>();
            services.AddHostedService<UserStatusListener>();

            return services;
        }
    }
}
