using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TehGM.EinherjiBot.CommandsProcessing;
using TehGM.EinherjiBot.DiscordClient;

namespace TehGM.EinherjiBot.Administration
{
    [LoadRegexCommands]
    [HelpCategory("Special", -99999)]
    [PersistentModule(PreInitialize = true)]
    public class AdminCommandsHandler : IDisposable
    {
        private readonly DiscordSocketClient _client;
        private readonly IOptionsMonitor<EinherjiOptions> _einherjiOptions;
        private readonly ILogger _log;

        private readonly CancellationTokenSource _hostCts;

        public AdminCommandsHandler(DiscordSocketClient client, ILogger<AdminCommandsHandler> log,
            IOptionsMonitor<EinherjiOptions> einherjiOptions)
        {
            this._client = client;
            this._log = log;
            this._einherjiOptions = einherjiOptions;
            this._hostCts = new CancellationTokenSource();

            this._client.UserLeft += OnUserLeftAsync;
        }

        protected Task OnUserLeftAsync(SocketGuild guild, SocketUser user)
        {
            if (guild.SystemChannel == null)
                return Task.CompletedTask;
            EmbedBuilder embed = new EmbedBuilder()
                .WithDescription($"**{user.Mention}** *(`{user.Username}#{user.Discriminator}`)* **has left.**")
                .WithColor((Color)System.Drawing.Color.Cyan);
            return guild.SystemChannel.SendMessageAsync(null, false, embed.Build(), _hostCts.Token);
        }

        public void Dispose()
        {
            try { this._hostCts?.Cancel(); } catch { }
            try { this._hostCts?.Dispose(); } catch { }
            try { this._client.UserLeft -= OnUserLeftAsync; } catch { }
        }
    }
}
