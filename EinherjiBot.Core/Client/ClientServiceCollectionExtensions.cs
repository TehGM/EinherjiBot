using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

            services.TryAddSingleton<IHostedDiscordClient, HostedDiscordClient>();
            services.AddTransient<IHostedService>(s => (IHostedService)s.GetRequiredService<IHostedDiscordClient>());
            services.TryAddSingleton<IDiscordClient>(s => s.GetRequiredService<IHostedDiscordClient>().Client);
            services.TryAddSingleton<DiscordSocketClient>(s => (DiscordSocketClient)s.GetRequiredService<IDiscordClient>());

            return services;
        }
    }
}
