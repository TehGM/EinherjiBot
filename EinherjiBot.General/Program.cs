﻿global using TehGM.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using System.Threading.Tasks;
using TehGM.EinherjiBot.Administration;
using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.CommandsProcessing;
using TehGM.EinherjiBot.EliteDangerous;
using TehGM.EinherjiBot.EliteDangerous.Services;
using TehGM.EinherjiBot.GameServers;
using TehGM.EinherjiBot.GameServers.Services;
using TehGM.EinherjiBot.Kathara;
using TehGM.EinherjiBot.Logging;
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
            // add default logger for errors that happen before host runs
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/unhandled.log",
                fileSizeLimitBytes: 1048576,        // 1MB
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 5,
                rollingInterval: RollingInterval.Day)
                .CreateLogger();
            LoggingConfiguration.StartLoggingUnhandledExceptions();

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureSecretsFiles()
                .UseSerilog(ConfigureSerilog, true)
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

        public static void ConfigureSerilog(HostBuilderContext context, LoggerConfiguration config)
        {
            config.ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext();
            DatadogOptions ddOptions = context.Configuration.GetSection("Serilog")?.GetSection("DataDog")?.Get<DatadogOptions>();
            if (!string.IsNullOrWhiteSpace(ddOptions?.ApiKey))
            {
                config.WriteTo.DatadogLogs(
                    ddOptions.ApiKey,
                    source: ".NET",
                    service: ddOptions.ServiceName ?? "Einherji",
                    host: ddOptions.HostName ?? Environment.MachineName,
                    new string[] {
                                $"env:{(ddOptions.EnvironmentName ?? context.HostingEnvironment.EnvironmentName)}",
                                $"assembly:{(ddOptions.AssemblyName ?? context.HostingEnvironment.ApplicationName)}"
                    },
                    ddOptions.ToDatadogConfiguration(),
                    // no need for debug logs in datadag
                    logLevel: ddOptions.OverrideLogLevel ?? LogEventLevel.Information
                );
            }
        }
    }
}
