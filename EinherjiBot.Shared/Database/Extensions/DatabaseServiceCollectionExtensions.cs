using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.Database;
using TehGM.EinherjiBot.Database.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DatabaseServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoConnection(this IServiceCollection services, Action<DatabaseOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure != null)
                services.Configure(configure);

            services.TryAddSingleton<IMongoConnection, MongoConnection>();

            return services;
        }
    }
}
