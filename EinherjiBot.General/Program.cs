using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Globalization;
using System.Threading.Tasks;
using TehGM.EinherjiBot.Administration;
using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.Client;
using TehGM.EinherjiBot.CommandsProcessing;
using TehGM.EinherjiBot.EliteDangerous;
using TehGM.EinherjiBot.Kathara;
using TehGM.EinherjiBot.Netflix;
using TehGM.EinherjiBot.Netflix.Services;
using TehGM.EinherjiBot.Patchbot;
using TehGM.EinherjiBot.Services;
using TehGM.EinherjiBot.Stellaris.Services;

namespace TehGM.EinherjiBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                Formatting = Formatting.None,
                Culture = CultureInfo.InvariantCulture
            };

            LoggingInitializationExtensions.EnableUnhandledExceptionLogging();

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureSecretsFiles()
                .ConfigureSerilog()
                .ConfigureServices((context, services) =>
                {
                    // configure options
                    services.Configure<EinherjiOptions>(context.Configuration);
                    services.Configure<CachingOptions>(MongoUserDataStore.CacheOptionName, context.Configuration.GetSection("Caching").GetSection(MongoUserDataStore.CacheOptionName));
                    services.Configure<CachingOptions>(MongoNetflixAccountStore.CacheOptionName, context.Configuration.GetSection("Caching").GetSection(MongoNetflixAccountStore.CacheOptionName));
                    services.Configure<CachingOptions>(MongoStellarisModsStore.CacheOptionName, context.Configuration.GetSection("Caching").GetSection(MongoStellarisModsStore.CacheOptionName));
                    services.Configure<DiscordOptions>(context.Configuration.GetSection("Discord"));
                    services.Configure<CommandsOptions>(context.Configuration.GetSection("Discord").GetSection("Commands"));
                    services.Configure<NetflixAccountOptions>(context.Configuration.GetSection("Netflix"));
                    services.Configure<BotChannelsRedirectionOptions>(context.Configuration.GetSection("BotChannelsRedirection"));
                    services.Configure<PiholeOptions>(context.Configuration.GetSection("Kathara").GetSection("Pihole"));
                    services.Configure<PatchbotOptions>(context.Configuration.GetSection("Patchbot"));
                    services.Configure<CommunityGoalsOptions>(context.Configuration.GetSection("EliteCommunityGoals"));

                    // add framework services

                    // add custom services
                    services.AddDiscordClient();
                    services.AddCommands();

                    // add bot features
                    services.AddIntel();
                    services.AddNetflixAccount();
                    services.AddStellaris();
                    services.AddAdministration();
                    services.AddBotChannelsRedirection();
                    services.AddPihole();
                    services.AddPatchbot();
                    services.AddEliteCommunityGoals();
                })
                .Build();
            await host.RunAsync().ConfigureAwait(false);
        }
    }
}
