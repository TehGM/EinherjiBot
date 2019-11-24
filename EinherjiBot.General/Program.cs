using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using TehGM.EinherjiBot.Config;
using TehGM.EinherjiBot.Utilities;
using Serilog;

namespace TehGM.EinherjiBot
{
    class Program
    {
        private static BotInitializer _initializer;

        static async Task Main(string[] args)
        {
            // load configuration early - needed for datadog sink
            BotConfig config = await BotConfig.LoadAllAsync();

            // initialize logging
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            LogSeverity logLevel = Debugger.IsAttached ? LogSeverity.Debug : LogSeverity.Info;
            Logging.Default = Logging.CreateDefaultConfiguration(logLevel)
                .WriteTo.DatadogLogs(
                    config.Auth.DatadogAPI.ApiKey, 
                    config.Auth.DatadogAPI.Source,
                    config.Auth.DatadogAPI.Service,
                    config.Auth.DatadogAPI.Host,
                    config.Auth.DatadogAPI.Tags,
                    config.Auth.DatadogAPI.ToDatadogConfiguration())
                .CreateLogger();

            // initialize bot
            _initializer = new BotInitializer();
            _initializer.LogLevel = logLevel;
            await _initializer.StartClient(config);
            _initializer.Client.Connected += Client_Connected;
            await Task.Delay(-1);
        }

        private static Task Client_Connected()
        {
            return _initializer.Client.SetGameAsync("TehGM's orders", null, ActivityType.Listening);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Logging.Default.Fatal((Exception)e.ExceptionObject, "Unhandled exception");
            }
            catch { }
        }
    }
}
