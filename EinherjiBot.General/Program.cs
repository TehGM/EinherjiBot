using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using TehGM.EinherjiBot.Client;
using TehGM.EinherjiBot.CommandsProcessing;
using TehGM.EinherjiBot.Netflix;

namespace TehGM.EinherjiBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            LoggingInitializationExtensions.EnableUnhandledExceptionLogging();

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureSecretsFiles()
                .ConfigureSerilog()
                .ConfigureServices((context, services) =>
                {
                    // configure options
                    services.Configure<EinherjiOptions>(context.Configuration);
                    services.Configure<DiscordOptions>(context.Configuration.GetSection("Discord"));
                    services.Configure<CommandsOptions>(context.Configuration.GetSection("Discord").GetSection("Commands"));
                    services.Configure<NetflixAccountOptions>(context.Configuration.GetSection("Netflix"));

                    // add framework services

                    // add custom services
                    services.AddDiscordClient();
                    services.AddCommands();

                    services.AddNetflixAccount();
                })
                .Build();
            await host.RunAsync().ConfigureAwait(false);
        }
    }
}
