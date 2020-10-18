﻿using System;
using TehGM.EinherjiBot.CommandsProcessing;
using TehGM.EinherjiBot.CommandsProcessing.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CommandsServiceCollectionExtensions
    {
        public static IServiceCollection AddCommands(this IServiceCollection services, Action<CommandOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure != null)
                services.Configure(configure);

            services.AddHostedService<SimpleCommandHandler>();
            services.AddHostedService<RegexCommandHandler>();

            return services;
        }
    }
}
