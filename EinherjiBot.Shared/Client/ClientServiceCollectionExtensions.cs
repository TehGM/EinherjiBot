﻿using System;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using TehGM.EinherjiBot;
using TehGM.EinherjiBot.Client;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ClientServiceCollectionExtensions
    {
        public static IServiceCollection AddDiscordClient(this IServiceCollection services, Action<DiscordOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure != null)
                services.Configure(configure);

            services.AddSingleton<IHostedDiscordClient, HostedDiscordClient>();
            services.AddTransient<IHostedService>(s => (IHostedService)s.GetRequiredService<IHostedDiscordClient>());
            services.AddTransient<IDiscordClient>(s => s.GetRequiredService<IHostedDiscordClient>().Client);
            services.AddTransient<DiscordSocketClient>(s => (DiscordSocketClient)s.GetRequiredService<IDiscordClient>());

            return services;
        }
    }
}
