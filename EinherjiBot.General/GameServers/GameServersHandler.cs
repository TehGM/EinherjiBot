using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TehGM.EinherjiBot.CommandsProcessing;
using System.Linq;
using Discord.Commands;
using System.Text.RegularExpressions;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using TehGM.EinherjiBot.DiscordClient;

namespace TehGM.EinherjiBot.GameServers
{
    [LoadRegexCommands]
    [HelpCategory("Games", 10)]
    public class GameServersHandler
    {
        private readonly IGameServerStore _gameServersStore;
        private readonly GameServersOptions _gameServersOptions;
        private readonly ILogger _log;

        private bool IsAutoRemoving => _gameServersOptions?.AutoRemoveDelay > TimeSpan.Zero;

        public GameServersHandler(ILogger<GameServersHandler> log, IGameServerStore gameServersStore, IOptionsSnapshot<GameServersOptions> gameServersOptions)
        {
            this._gameServersStore = gameServersStore;
            this._gameServersOptions = gameServersOptions.Value;
            this._log = log;
        }

        [RegexCommand(@"^server(?:\s(.+))?")]
        [Name("server <game>")]
        [Summary("If you're authorized, will give you info how to connect to our game servers.")]
        [Priority(-18)]
        [RestrictCommand]
        private async Task CmdGetAsync(SocketCommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            // check if command has game name
            using IDisposable logScope = _log.BeginCommandScope(context, this);
            if (match.Groups.Count < 2 || match.Groups[1]?.Length < 1)
            {
                await SendNameRequiredAsync(context.Channel, cancellationToken).ConfigureAwait(false);
                return;
            }

            // get server info
            string gameName = match.Groups[1].Value.Trim();
            GameServer server = await _gameServersStore.GetAsync(gameName, cancellationToken).ConfigureAwait(false);
            if (server == null)
            {
                _log.LogDebug("Server for game {Game} not found", gameName);
                await SendServerNotFoundAsync(context.Channel, gameName, cancellationToken).ConfigureAwait(false);
                return;
            }

            // check permissions
            if (!await IsAuthorizedAsync(context, server).ConfigureAwait(false))
            {
                _log.LogTrace("User {UserID} not authorized for server for game {Game} not found", context.User.Id, gameName);
                await SendUnatuthorizedAsync(context.Channel, server, cancellationToken).ConfigureAwait(false);
                return;
            }

            // build server info embed
            EmbedBuilder embed = new EmbedBuilder();
            embed.Title = $"{server.Game} Server Info";
            embed.Color = Color.Blue;
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(server.RulesURL))
                builder.AppendFormat("Before connecting, please read the [server rules]({0}).\n\n", server.RulesURL);
            builder.AppendFormat("***Address***: `{0}`\n", server.Address);
            if (!string.IsNullOrWhiteSpace(server.Password))
                builder.AppendFormat("***Password***: `{0}`\n", server.Password);
            embed.Description = builder.ToString();

            // send response - directly if PM, or direct to user's PM if in guild
            string text = this.IsAutoRemoving ? GetAutoremoveText() : null;
            IUserMessage sentMsg;
            if (context.IsPrivate)
                sentMsg = await context.ReplyAsync(text, false, embed.Build(), cancellationToken).ConfigureAwait(false);
            else
            {
                _ = context.InlineReplyAsync($"{ResponseEmote.SuccessSymbol} I will send you a private message with info on how to connect to the server!");
                Task<IUserMessage> pmTask = context.User.SendMessageAsync(text, false, embed.Build(), new RequestOptions { CancelToken = cancellationToken });
                sentMsg = await pmTask.ConfigureAwait(false);
            }

            // auto remove
            if (this.IsAutoRemoving)
                RemoveMessagesDelayed(_gameServersOptions.AutoRemoveDelay, cancellationToken, sentMsg);
        }   

        private async ValueTask<bool> IsAuthorizedAsync(SocketCommandContext context, GameServer server)
        {
            if (server.IsPublic)
                return true;
            if (server.AuthorizedUserIDs.Contains(context.User.Id))
                return true;

            // scan server roles
            foreach (ulong guildID in _gameServersOptions.RoleScanGuildIDs)
            {
                SocketGuild guild = context.Client.GetGuild(guildID);
                if (guild == null)
                    continue;
                IGuildUser guildUser = await guild.GetGuildUserAsync(context.User.Id).ConfigureAwait(false);
                if (guildUser == null)
                    continue;
                IEnumerable<IRole> roles = guildUser.GetRoles(role => server.AuthorizedRoleIDs.Contains(role.Id));
                if (roles.Any())
                    return true;
            }
            return false;
        }

        private string GetAutoremoveText()
            => $"I will remove this message in {_gameServersOptions.AutoRemoveDelay.ToShortFriendlyString()}.";
        private void RemoveMessagesDelayed(TimeSpan delay, CancellationToken cancellationToken, params IMessage[] messages)
        {
            if (messages.Length == 0)
                return;
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    Task[] tasks = new Task[messages.Length];
                    for (int i = 0; i < messages.Length; i++)
                    {
                        IMessage msg = messages[i];
                        tasks[i] = msg.Channel.DeleteMessageAsync(msg, cancellationToken);
                    }
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
                catch (OperationCanceledException) { _log.LogWarning("Auto-removal of Game Server message canceled"); }
                catch (Exception ex) when (ex.LogAsError(_log, "Exception occured when auto-removing Game Server message")) { }
            }, cancellationToken).ConfigureAwait(false);
        }

        private Task SendNameRequiredAsync(ISocketMessageChannel channel, CancellationToken cancellationToken = default)
            => channel.SendMessageAsync($"{ResponseEmote.FailureSymbol} Please specify game name.", cancellationToken);
        private Task SendServerNotFoundAsync(ISocketMessageChannel channel, string gameName, CancellationToken cancellationToken = default)
            => channel.SendMessageAsync($"{ResponseEmote.FailureSymbol} Server for game `{gameName}` not found!", cancellationToken);
        private Task SendUnatuthorizedAsync(ISocketMessageChannel channel, GameServer server, CancellationToken cancellationToken = default)
            => channel.SendMessageAsync($"{ResponseEmote.FailureSymbol} You're not authorized to access {server.Game} server.", cancellationToken);
    }
}
