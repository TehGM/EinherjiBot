global using TehGM.Utilities;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using TehGM.EinherjiBot.Administration;
using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.CommandsProcessing;
using TehGM.EinherjiBot.EliteDangerous;
using TehGM.EinherjiBot.EliteDangerous.Services;
using TehGM.EinherjiBot.GameServers;
using TehGM.EinherjiBot.GameServers.Services;
using TehGM.EinherjiBot.Kathara;
using TehGM.EinherjiBot.Netflix;
using TehGM.EinherjiBot.Netflix.Services;
using TehGM.EinherjiBot.Patchbot;
using TehGM.EinherjiBot.Patchbot.Services;
using TehGM.EinherjiBot.RandomStatus;

namespace TehGM.EinherjiBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            LoggingConfiguration.EnableUnhandledExceptionLogging();

            IHost host = Host.CreateDefaultBuilder(args)
                //.ConfigureAppConfiguration((context, builder) =>
                //{
                //    context.
                //})
                .ConfigureSecretsFiles()
                .ConfigureSerilog()
                .ConfigureServices((context, services) =>
                {
                    // configure options
                    services.Configure<EinherjiOptions>(context.Configuration);
                    services.Configure<Database.MongoOptions>(context.Configuration.GetSection("Database"));
                    services.Configure<DiscordClient.DiscordOptions>(context.Configuration.GetSection("Discord"));
                    services.Configure<CommandsOptions>(context.Configuration.GetSection("Discord").GetSection("Commands"));
                    services.Configure<NetflixAccountOptions>(context.Configuration.GetSection("Netflix"));
                    services.Configure<BotChannelsRedirectionOptions>(context.Configuration.GetSection("BotChannelsRedirection"));
                    services.Configure<PiholeOptions>(context.Configuration.GetSection("Kathara").GetSection("Pihole"));
                    services.Configure<PatchbotOptions>(context.Configuration.GetSection("Patchbot"));
                    services.Configure<CommunityGoalsOptions>(context.Configuration.GetSection("EliteCommunityGoals"));
                    services.Configure<RandomStatusOptions>(context.Configuration.GetSection("RandomStatus"));
                    services.Configure<GameServersOptions>(context.Configuration.GetSection("GameServers"));

                    // add framework services

                    // add custom services
                    services.AddDiscordClient();
                    services.AddCommands();
                    services.AddEntityCaching();

                    // add bot features
                    services.AddUserIntel();
                    services.AddNetflixAccount();
                    services.AddAdministration();
                    services.AddBotChannelsRedirection();
                    services.AddPihole();
                    services.AddPatchbot();
                    services.AddEliteCommunityGoals();
                    services.AddGameServers();
                    services.AddRandomStatus();
                })
                .Build();
            await host.RunAsync().ConfigureAwait(false);
        }
    }
}
