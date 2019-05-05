using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using TehGM.EinherjiBot.CommandsProcessing;
using TehGM.EinherjiBot.Config;
using TehGM.EinherjiBot.Extensions;

namespace TehGM.EinherjiBot
{
    public class BotInitializer : IDisposable
    {
        public DiscordSocketClient Client { get; private set; }
        public BotConfig Config { get; private set; }
        public IList<HandlerBase> Handlers { get; private set; }

        public bool LoadConfigs { get; set; } = true;
        public bool HandleLogs { get; set; } = true;
        public LogSeverity DebuggingLogLevel { get; set; } = LogSeverity.Debug;
        public LogSeverity ProductionLogLevel { get; set; } = LogSeverity.Info;
        public bool AutoLoadHandlers { get; set; } = true;

        public virtual async Task<DiscordSocketClient> StartClient()
        {
            DiscordSocketConfig clientConfig = new DiscordSocketConfig();
            clientConfig.WebSocketProvider = Discord.Net.WebSockets.DefaultWebSocketProvider.Instance;
            clientConfig.LogLevel = Debugger.IsAttached ? DebuggingLogLevel : ProductionLogLevel;
            Client = new DiscordSocketClient(clientConfig);
            Config = await BotConfig.LoadAllAsync();

            Client.Log += Client_Log;

            if (AutoLoadHandlers && Handlers == null)
                Handlers = InitializeHandlers(Client, Config);

            await Client.LoginAsync(TokenType.Bot, Config.Auth.Token);
            await Client.StartAsync();
            return Client;
        }

        private Task Client_Log(LogMessage arg)
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }

        public static IList<HandlerBase> InitializeHandlers(DiscordSocketClient client, BotConfig config)
        {
            Type[] types = Assembly.GetExecutingAssembly().FindDerivedTypes(typeof(HandlerBase));
            List<HandlerBase> handlers = new List<HandlerBase>(types.Length);

            for (int i = 0; i < types.Length; i++)
            {
                HandlerBase handler = (HandlerBase)Activator.CreateInstance(types[i], client, config);
                handlers.Add(handler);
            }

            return handlers;
        }

        public virtual void Dispose()
        {
            Client.Log -= Client_Log;
        }
    }
}
