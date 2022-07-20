using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.Auditing;
using TehGM.EinherjiBot.Auditing.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuditingServiceCollectionExtensions
    {
        public static IServiceCollection AddBotAudits(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddMongoDB();
            services.TryAddSingleton(typeof(IAuditStore<>), typeof(MongoAuditStore<>));

            return services;
        }
    }
}
