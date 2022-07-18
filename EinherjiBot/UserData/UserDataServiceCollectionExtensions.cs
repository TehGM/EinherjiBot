using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot;
using TehGM.EinherjiBot.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class UserDataServiceCollectionExtensions
    {
        public static IServiceCollection AddUserData(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddMongoDB();
            services.AddEntityCaching();
            services.TryAddSingleton<IUserDataStore, MongoUserDataStore>();

            return services;
        }
    }
}
