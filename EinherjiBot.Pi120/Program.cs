using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using TehGM.EinherjiBot.Client;
using TehGM.EinherjiBot.CommandsProcessing;

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
                    services.Configure<DiscordOptions>(context.Configuration.GetSection("Discord"));
                    services.Configure<CommandsOptions>(context.Configuration.GetSection("Discord").GetSection("Commands"));

                    // add framework services

                    // add custom services
                    services.AddDiscordClient();
                    services.AddCommands();
                })
                .Build();
            await host.RunAsync().ConfigureAwait(false);
        }
    }
}
