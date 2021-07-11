using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using TehGM.EinherjiBot.CommandsProcessing;
using TehGM.EinherjiBot.CommandsProcessing.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CommandsServiceCollectionExtensions
    {
        public static IServiceCollection AddCommands(this IServiceCollection services, Action<CommandsOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure != null)
                services.Configure(configure);

            services.TryAddSingleton<IRegexCommandModuleProvider, RegexComandModuleProvider>();
            services.TryAddEnumerable(new ServiceDescriptor[] 
            {
                ServiceDescriptor.Transient<IHostedService, SimpleCommandHandler>(),
                ServiceDescriptor.Transient<IHostedService, RegexCommandHandler>()
            });

            return services;
        }
    }
}
