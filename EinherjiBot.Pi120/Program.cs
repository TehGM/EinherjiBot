using Discord;
using System.Diagnostics;
using System.Threading.Tasks;
using TehGM.EinherjiBot.Utilities;

namespace TehGM.EinherjiBot
{
    class Program
    {
        private static BotInitializer _initializer;

        static async Task Main(string[] args)
        {
            // initialize logging
            LogSeverity logLevel = Debugger.IsAttached ? LogSeverity.Verbose : LogSeverity.Info;
            Logging.Default = Logging.CreateDefaultConfiguration()
                .MinimumLevel.Is(Logging.SeverityToSerilogLevel(logLevel))      // convert Discord.NET severity to serilog level to keep it consistent
                .CreateLogger();

            // initialize bot
            _initializer = new BotInitializer();
            _initializer.LogLevel = logLevel;
            await _initializer.StartClient();
            await Task.Delay(-1);
        }
    }
}
