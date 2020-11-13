using System;
using System.Collections.Generic;
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

namespace TehGM.EinherjiBot.Patchbot
{
    [LoadRegexCommands]
    [PersistentModule(PreInitialize = true)]
    public class PatchbotHandler : IDisposable
    {
        private const char _namesSeparator = '|';
        private readonly DiscordSocketClient _client;
        private readonly IPatchbotGamesStore _patchbotGamesStore;
        private readonly IOptionsMonitor<EinherjiOptions> _einherjiOptions;
        private readonly IOptionsMonitor<PatchbotOptions> _patchbotOptions;
        private readonly CancellationTokenSource _hostCts;
        private readonly ILogger _log;

        public PatchbotHandler(DiscordSocketClient client, ILogger<PatchbotHandler> log, IPatchbotGamesStore patchbotGameStore,
            IOptionsMonitor<EinherjiOptions> einherjiOptions, IOptionsMonitor<PatchbotOptions> patchbotOptions)
        {
            this._client = client;
            this._patchbotGamesStore = patchbotGameStore;
            this._einherjiOptions = einherjiOptions;
            this._patchbotOptions = patchbotOptions;
            this._log = log;

            this._client.MessageReceived += OnClientMessageReceivedAsync;
        }

        protected Task OnClientMessageReceivedAsync(SocketMessage message)
        {
            // verify this is a webhook messages
            if (!message.Author.IsWebhook)
                return Task.CompletedTask;

            // verify this is a valid channel
            if (_patchbotOptions.CurrentValue.ChannelIDs?.Contains(message.Channel.Id) != true)
                return Task.CompletedTask;
            if (!(message.Channel is SocketTextChannel))
                return Task.CompletedTask;

            // determine type of the webhook
            if (_patchbotOptions.CurrentValue.PatchbotWebhookIDs?.Contains(message.Author.Id) == true)
                return ProcessPatchbotMessageAsync(message, _hostCts.Token);
            return ProcessFollowedChannelMessageAsync(message, _hostCts.Token);
        }

        private Task ProcessPatchbotMessageAsync(SocketMessage message, CancellationToken cancellationToken = default)
        {
            if (message.Embeds.Count == 0)
                return Task.CompletedTask;

            // get game from embed author text
            Embed embed = message.Embeds.First();
            string gameName = embed.Author?.Name;

            _log.LogTrace("Received Patchbot webhook for game {GameName}", gameName);
            return PingGameAsync(message, gameName, cancellationToken);
        }

        private async Task ProcessFollowedChannelMessageAsync(SocketMessage message, CancellationToken cancellationToken = default)
        {
            if (!(message is SocketUserMessage msg))
                return;
            // attempt to get nickname, but use username if unavailable
            string authorName = message.Author.Username;
            SocketGuild guild = (msg.Channel as SocketGuildChannel)?.Guild;
            if (guild != null)
            {
                SocketGuildUser guildUser = await guild.GetGuildUserAsync(message.Author).ConfigureAwait(false);
                if (guildUser != null)
                    authorName = guildUser.Nickname ?? authorName;
            }

            // trim to #
            int hashIndex = authorName.IndexOf('#');
            string gameName = hashIndex < 0 ? authorName : authorName.Remove(hashIndex).TrimEnd();

            _log.LogTrace("Received followed channel webhook for game {GameName}", gameName);
            await PingGameAsync(message, gameName, cancellationToken);
        }

        private async Task PingGameAsync(SocketMessage message, string gameName, CancellationToken cancellationToken = default)
        {
            PatchbotGame game = await _patchbotGamesStore.GetAsync(gameName, cancellationToken).ConfigureAwait(false);
            if (game == null)
            {
                _log.LogWarning("Patchbot game {GameName} not found", gameName);
                return;
            }
            // if no one subscribes to this game, abort
            if (game.SubscriberIDs.Count == 0)
                return;

            // get only subscribers that are present in this channel
            IEnumerable<SocketGuildUser> presentSubscribers = (message.Channel as SocketGuildChannel).Users.Where(user => game.SubscriberIDs.Contains(user.Id));

            // abort if none of the users has access to the channel
            if (!presentSubscribers.Any())
                return;

            // ping them all
            _log.LogDebug("Pinging patchbot game {GameName} to {UsersCount} subscribers", game.Name, presentSubscribers.Count());
            await message.ReplyAsync($"{string.Join(' ', presentSubscribers.Select(user => user.Mention))}\n{message.GetJumpUrl()}", cancellationToken).ConfigureAwait(false);
        }

        [RegexCommand(@"^patchbot sub(?:scribe)?(?: (.+))?")]
        private async Task CmdSubscribeAsync(SocketCommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = _log.BeginCommandScope(context, this);
            if (match.Groups.Count < 2 || match.Groups[1]?.Length < 1)
            {
                await SendNameRequiredAsync(context.Channel, cancellationToken).ConfigureAwait(false);
                return;
            }
            string gameName = match.Groups[1].Value.Trim();
            _log.LogDebug("Subscribing user {UserID} to patchbot game {GameName}", context.User.Id, gameName);
            PatchbotGame game = await _patchbotGamesStore.GetAsync(gameName, cancellationToken).ConfigureAwait(false);
            if (game == null)
            {
                _log.LogDebug("Patchbot game {GameName} not found", gameName);
                await SendGameNotFoundAsync(context.Channel, gameName, cancellationToken).ConfigureAwait(false);
                return;
            }
            if (game.SubscriberIDs.Add(context.User.Id))
                await _patchbotGamesStore.SetAsync(game, cancellationToken).ConfigureAwait(false);
            await context.ReplyAsync($"{_einherjiOptions.CurrentValue.SuccessSymbol} You will now get pinged about `{game.Name}` updates.", cancellationToken).ConfigureAwait(false);
        }

        [RegexCommand(@"^patchbot unsub(?:scribe)?(?: (.+))?")]
        private async Task CmdUnsubscribeAsync(SocketCommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = _log.BeginCommandScope(context, this);
            if (match.Groups.Count < 2 || match.Groups[1]?.Length < 1)
            {
                await SendNameRequiredAsync(context.Channel, cancellationToken).ConfigureAwait(false);
                return;
            }
            string gameName = match.Groups[1].Value.Trim();
            _log.LogDebug("Unsubscribing user {UserID} to patchbot game {GameName}", context.User.Id, gameName);
            PatchbotGame game = await _patchbotGamesStore.GetAsync(gameName, cancellationToken).ConfigureAwait(false);
            if (game == null)
            {
                _log.LogDebug("Patchbot game {GameName} not found", gameName);
                await SendGameNotFoundAsync(context.Channel, gameName, cancellationToken).ConfigureAwait(false);
                return;
            }
            if (game.SubscriberIDs.Remove(context.User.Id))
                await _patchbotGamesStore.DeleteAsync(game, cancellationToken).ConfigureAwait(false);
            await context.ReplyAsync($"{_einherjiOptions.CurrentValue.SuccessSymbol} You will no longer be pinged about `{game.Name}` updates.", cancellationToken).ConfigureAwait(false);
        }

        [RegexCommand(@"^patchbot add (?: (.+))?")]
        private async Task CmdAddGameAsync(SocketCommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = _log.BeginCommandScope(context, this);
            if (context.User.Id != _einherjiOptions.CurrentValue.AuthorID)
            {
                await SendInsufficientPermissionsAsync(context.Channel, cancellationToken).ConfigureAwait(false);
                return;
            }
            // get names
            if (match.Groups.Count < 2 || match.Groups[1]?.Length < 1)
            {
                await SendNameAndAliasesRequiredAsync(context.Channel, cancellationToken).ConfigureAwait(false);
                return;
            }
            string[] names = match.Groups[1].Value.Split(_namesSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Select(name => name.Trim()).Where(name => !string.IsNullOrWhiteSpace(name)).ToArray();
            if (names.Length == 0)
            {
                await SendNameAndAliasesRequiredAsync(context.Channel, cancellationToken).ConfigureAwait(false);
                return;
            }

            // check if game doesn't yet exist
            string gameName = names.First();
            PatchbotGame game = await _patchbotGamesStore.GetAsync(gameName, cancellationToken).ConfigureAwait(false);
            if (game == null)
            {
                _log.LogDebug("Creating patchbot game {GameName}", gameName);
                game = new PatchbotGame(names.First(), names.TakeLast(names.Length - 1));
            }
            // if it does, just add new aliases
            else
            {
                _log.LogDebug("Adding {AliasesCount} aliases to patchbot game {GameName}", names.Length, game.Name);
                for (int i = 0; i < names.Length; i++)
                {
                    if (game.Aliases.Contains(names[i], StringComparer.OrdinalIgnoreCase))
                        continue;
                    game.Aliases.Add(names[i]);
                }
            }

            await _patchbotGamesStore.SetAsync(game, cancellationToken).ConfigureAwait(false);
            await context.ReplyAsync($"{_einherjiOptions.CurrentValue.SuccessSymbol} Game `{game.Name}` updated.", cancellationToken).ConfigureAwait(false);
        }

        [RegexCommand(@"^patchbot (?:remove|del|delete) (?: (.+))?")]
        private async Task CmdRemoveGameAsync(SocketCommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = _log.BeginCommandScope(context, this);
            if (context.User.Id != _einherjiOptions.CurrentValue.AuthorID)
            {
                await SendInsufficientPermissionsAsync(context.Channel, cancellationToken).ConfigureAwait(false);
                return;
            }
            if (match.Groups.Count < 2 || match.Groups[1]?.Length < 1)
            {
                await SendNameRequiredAsync(context.Channel, cancellationToken).ConfigureAwait(false);
                return;
            }


            // check if game exists
            string gameName = match.Groups[1].Value.Trim();
            _log.LogDebug("Creating patchbot game {GameName}", gameName);
            PatchbotGame game = await _patchbotGamesStore.GetAsync(gameName, cancellationToken).ConfigureAwait(false);
            if (game == null)
            {
                _log.LogDebug("Patchbot game {GameName} not found", gameName);
                await SendGameNotFoundAsync(context.Channel, gameName, cancellationToken).ConfigureAwait(false);
                return;
            }

            await _patchbotGamesStore.DeleteAsync(game, cancellationToken).ConfigureAwait(false);
            await context.ReplyAsync($"{_einherjiOptions.CurrentValue.SuccessSymbol} Game `{game.Name}` removed.", cancellationToken).ConfigureAwait(false);
        }

        private Task SendInsufficientPermissionsAsync(ISocketMessageChannel channel, CancellationToken cancellationToken = default)
            => channel.SendMessageAsync($"{_einherjiOptions.CurrentValue.FailureSymbol} Insufficient permissions.", cancellationToken);
        private Task SendGameNotFoundAsync(ISocketMessageChannel channel, string gameName, CancellationToken cancellationToken = default)
            => channel.SendMessageAsync($"{_einherjiOptions.CurrentValue.FailureSymbol} Game `{gameName}` not found!", cancellationToken);
        private Task SendIDNotValidAsync(ISocketMessageChannel channel, string value, CancellationToken cancellationToken = default)
            => channel.SendMessageAsync($"{_einherjiOptions.CurrentValue.FailureSymbol} `{value}` is not a valid webhook/bot ID!", cancellationToken);
        private Task SendNameAndAliasesRequiredAsync(ISocketMessageChannel channel, CancellationToken cancellationToken = default)
            => channel.SendMessageAsync($"{_einherjiOptions.CurrentValue.FailureSymbol} Please specify game name and aliases (separated with `{_namesSeparator}`).", cancellationToken);
        private Task SendNameRequiredAsync(ISocketMessageChannel channel, CancellationToken cancellationToken = default)
            => channel.SendMessageAsync($"{_einherjiOptions.CurrentValue.FailureSymbol} Please specify game name.", cancellationToken);

        public void Dispose()
        {
            try { this._hostCts?.Cancel(); } catch { }
            try { this._hostCts?.Dispose(); } catch { }
            try { this._client.MessageReceived -= OnClientMessageReceivedAsync; } catch { }
        }
    }
}
