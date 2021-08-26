using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TehGM.EinherjiBot.CommandsProcessing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Text;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Drawing;

namespace TehGM.EinherjiBot.GameServers
{
    [RegexCommandsModule]
    [HelpCategory("Games", 10)]
    public class GameServersHandler
    {
        private readonly IGameServerStore _gameServersStore;
        private readonly EinherjiOptions _einherjiOptions;
        private readonly GameServersOptions _gameServersOptions;
        private readonly ILogger _log;

        private bool IsAutoRemoving => _gameServersOptions?.AutoRemoveDelay > TimeSpan.Zero;

        public GameServersHandler(ILogger<GameServersHandler> log, IGameServerStore gameServersStore,
            IOptionsSnapshot<EinherjiOptions> einherjiOptions, IOptionsSnapshot<GameServersOptions> gameServersOptions)
        {
            this._gameServersStore = gameServersStore;
            this._einherjiOptions = einherjiOptions.Value;
            this._gameServersOptions = gameServersOptions.Value;
            this._log = log;
        }

        [RegexCommand(@"^server(?:\s(.+))?")]
        [Name("server <game>")]
        [Summary("If you're authorized, will give you info how to connect to our game servers.")]
        [Priority(-18)]
        [RestrictCommand]
        private async Task CmdGetAsync(CommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            // check if command has game name
            if (match.Groups.Count < 2 || match.Groups[1]?.Length < 1)
            {
                await context.ReplyAsync($"{_einherjiOptions.FailureSymbol} Please specify game name.").ConfigureAwait(false);
                return;
            }

            // get server info
            string gameName = match.Groups[1].Value.Trim();
            GameServer server = await _gameServersStore.GetAsync(gameName, cancellationToken).ConfigureAwait(false);
            if (server == null)
            {
                _log.LogDebug("Server for game {Game} not found", gameName);
                await context.ReplyAsync($"{_einherjiOptions.FailureSymbol} Server for game `{gameName}` not found!").ConfigureAwait(false);
                return;
            }

            // check permissions
            if (!await IsAuthorizedAsync(context, server).ConfigureAwait(false))
            {
                _log.LogTrace("User {UserID} not authorized for server for game {Game} not found", context.User.Id, gameName);
                await context.ReplyAsync($"{_einherjiOptions.FailureSymbol} You're not authorized to access {server.Game} server.").ConfigureAwait(false);
                return;
            }

            // build server info embed
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();
            embed.Title = $"{server.Game} Server Info";
            embed.WithColor(Color.Blue);
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(server.RulesURL))
                builder.AppendFormat("Before connecting, please read the [server rules]({0}).\n\n", server.RulesURL);
            builder.AppendFormat("***Address***: `{0}`\n", server.Address);
            if (!string.IsNullOrWhiteSpace(server.Password))
                builder.AppendFormat("***Password***: `{0}`\n", server.Password);
            embed.Description = builder.ToString();

            // send response - directly if PM, or direct to user's PM if in guild
            string text = this.IsAutoRemoving ? GetAutoremoveText() : null;
            DiscordMessage sentMsg;
            if (context.Channel.IsPrivate)
                sentMsg = await context.ReplyAsync(text, embed.Build()).ConfigureAwait(false);
            else
            {
                _ = context.InlineReplyAsync($"{_einherjiOptions.SuccessSymbol} I will send you a private message with info on how to connect to the server!");
                DiscordMember user = await context.GetGuildMemberAsync().ConfigureAwait(false);
                Task<DiscordMessage> pmTask = user.SendMessageAsync(text, embed.Build());
                sentMsg = await pmTask.ConfigureAwait(false);
            }

            // auto remove
            if (this.IsAutoRemoving)
                RemoveMessagesDelayed(_gameServersOptions.AutoRemoveDelay, cancellationToken, sentMsg);
        }   

        private async Task<bool> IsAuthorizedAsync(CommandContext context, GameServer server)
        {
            if (server.IsPublic)
                return true;
            if (server.AuthorizedUserIDs.Contains(context.User.Id))
                return true;

            // scan server roles
            foreach (ulong guildID in _gameServersOptions.RoleScanGuildIDs)
            {
                DiscordGuild guild = await context.Client.GetGuildAsync(guildID);
                if (guild == null)
                    continue;
                DiscordMember guildUser = await guild.GetMemberAsync(context.User.Id).ConfigureAwait(false);
                if (guildUser == null)
                    continue;
                if (guildUser.Roles.Any(role => server.AuthorizedRoleIDs.Contains(role.Id)))
                    return true;
            }
            return false;
        }

        private string GetAutoremoveText()
            => $"I will remove this message in {_gameServersOptions.AutoRemoveDelay.ToShortFriendlyString()}.";
        private void RemoveMessagesDelayed(TimeSpan delay, CancellationToken cancellationToken, params DiscordMessage[] messages)
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
                        DiscordMessage msg = messages[i];
                        tasks[i] = msg.Channel.DeleteMessageAsync(msg);
                    }
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
                catch (OperationCanceledException) { _log.LogWarning("Auto-removal of Game Server message canceled"); }
                catch (Exception ex) when (ex.LogAsError(_log, "Exception occured when auto-removing Game Server message")) { }
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}
