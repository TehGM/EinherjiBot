﻿using Microsoft.Extensions.DependencyInjection.Extensions;
using TehGM.EinherjiBot.Administration.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AdministrationServiceCollectionExtensions
    {
        public static IServiceCollection AddAdministration(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddDiscordClient();
            services.AddHostedService<JoinLeaveNotifier>();

            return services;
        }
    }
}
