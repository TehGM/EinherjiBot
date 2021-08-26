using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TehGM.EinherjiBot.CommandsProcessing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;

namespace TehGM.EinherjiBot.Patchbot
{
    [LoadRegexCommands]
    [PersistentModule(PreInitialize = true)]
    [HelpCategory("Game Updates", 20)]
    public class PatchbotHandler : IDisposable
    {
        private const char _namesSeparator = '|';
        private readonly DiscordClient _client;
        private readonly IPatchbotGamesStore _patchbotGamesStore;
        private readonly IOptionsMonitor<EinherjiOptions> _einherjiOptions;
        private readonly IOptionsMonitor<PatchbotOptions> _patchbotOptions;
        private readonly CancellationTokenSource _hostCts;
        private readonly ILogger _log;

        public PatchbotHandler(DiscordClient client, ILogger<PatchbotHandler> log, IPatchbotGamesStore patchbotGameStore,
            IOptionsMonitor<EinherjiOptions> einherjiOptions, IOptionsMonitor<PatchbotOptions> patchbotOptions)
        {
            this._client = client;
            this._patchbotGamesStore = patchbotGameStore;
            this._einherjiOptions = einherjiOptions;
            this._patchbotOptions = patchbotOptions;
            this._log = log;
            this._hostCts = new CancellationTokenSource();

            this._client.MessageCreated += OnClientMessageReceivedAsync;
        }

        protected Task OnClientMessageReceivedAsync(DiscordClient client, MessageCreateEventArgs e)
        {
            // verify this is a webhook messages
            if (e.Message.WebhookId == null)
                return Task.CompletedTask;

            // verify this is a valid channel
            if (_patchbotOptions.CurrentValue.ChannelIDs?.Contains(e.Channel.Id) != true)
                return Task.CompletedTask;
            if (!e.Channel.IsGuildText())
                return Task.CompletedTask;

            // determine type of the webhook
            if (_patchbotOptions.CurrentValue.PatchbotWebhookIDs?.Contains(e.Author.Id) == true)
                return ProcessPatchbotMessageAsync(e.Message, _hostCts.Token);
            return ProcessFollowedChannelMessageAsync(e.Message, _hostCts.Token);
        }

        private Task ProcessPatchbotMessageAsync(DiscordMessage message, CancellationToken cancellationToken = default)
        {
            if (message.Embeds.Count == 0)
                return Task.CompletedTask;

            // get game from embed author text
            DiscordEmbed embed = message.Embeds.First();
            string gameName = embed.Author?.Name;

            _log.LogTrace("Received Patchbot webhook for game {GameName}", gameName);
            return PingGameAsync(message, gameName, cancellationToken);
        }

        private async Task ProcessFollowedChannelMessageAsync(DiscordMessage message, CancellationToken cancellationToken = default)
        {
            if (message.MessageType != MessageType.Default)
                return;
            // attempt to get nickname, but use username if unavailable
            string authorName = message.Author.Username;
            if (message.Channel.GuildId != null)
            {
                DiscordMember guildUser = await message.Channel.Guild.GetMemberAsync(message.Author.Id).ConfigureAwait(false);
                if (guildUser != null)
                    authorName = guildUser.Nickname ?? authorName;
            }

            // trim to #
            int hashIndex = authorName.IndexOf('#');
            string gameName = hashIndex < 0 ? authorName : authorName.Remove(hashIndex).TrimEnd();

            _log.LogTrace("Received followed channel webhook for game {GameName}", gameName);
            await PingGameAsync(message, gameName, cancellationToken);
        }

        private async Task PingGameAsync(DiscordMessage message, string gameName, CancellationToken cancellationToken = default)
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
            IEnumerable<DiscordMember> presentSubscribers = message.Channel.Users.Where(user => game.SubscriberIDs.Contains(user.Id));

            // abort if none of the users has access to the channel
            if (!presentSubscribers.Any())
                return;

            // ping them all
            _log.LogDebug("Pinging patchbot game {GameName} to {UsersCount} subscribers", game.Name, presentSubscribers.Count());
            DiscordMessageBuilder builder = new DiscordMessageBuilder();
            builder.Content = $"{string.Join(' ', presentSubscribers.Select(user => user.Mention))}\n{message.JumpLink}";
            builder.WithAllowedMentions(presentSubscribers.Select(u => new UserMention(u)).Cast<IMention>());
            await builder.SendAsync(message.Channel).ConfigureAwait(false);
        }

        [RegexCommand(@"^patchbot\ssub(?:scribe)?(?: (.+))?")]
        [Name("patchbot subscribe <game>")]
        [Summary("Subscribes you for pings whenever there's a patchbot update about *<game>*.")]
        [Priority(559)]
        [RestrictCommand]
        private async Task CmdSubscribeAsync(CommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = _log.BeginCommandScope(context, this);
            if (match.Groups.Count < 2 || match.Groups[1]?.Length < 1)
            {
                await SendNameRequiredAsync(context.Channel).ConfigureAwait(false);
                return;
            }
            string gameName = match.Groups[1].Value.Trim();
            _log.LogDebug("Subscribing user {UserID} to patchbot game {GameName}", context.User.Id, gameName);
            PatchbotGame game = await _patchbotGamesStore.GetAsync(gameName, cancellationToken).ConfigureAwait(false);
            if (game == null)
            {
                _log.LogDebug("Patchbot game {GameName} not found", gameName);
                await SendGameNotFoundAsync(context.Channel, gameName).ConfigureAwait(false);
                return;
            }
            if (game.SubscriberIDs.Add(context.User.Id))
                await _patchbotGamesStore.SetAsync(game, cancellationToken).ConfigureAwait(false);
            await context.ReplyAsync($"{_einherjiOptions.CurrentValue.SuccessSymbol} You will now get pinged about `{game.Name}` updates.").ConfigureAwait(false);
        }

        [RegexCommand(@"^patchbot\sunsub(?:scribe)?(?: (.+))?")]
        [Name("patchbot unsubscribe <game>")]
        [Summary("As above, but for cancelling your subscription.")]
        [Priority(558)]
        [RestrictCommand]
        private async Task CmdUnsubscribeAsync(CommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = _log.BeginCommandScope(context, this);
            if (match.Groups.Count < 2 || match.Groups[1]?.Length < 1)
            {
                await SendNameRequiredAsync(context.Channel).ConfigureAwait(false);
                return;
            }
            string gameName = match.Groups[1].Value.Trim();
            _log.LogDebug("Unsubscribing user {UserID} to patchbot game {GameName}", context.User.Id, gameName);
            PatchbotGame game = await _patchbotGamesStore.GetAsync(gameName, cancellationToken).ConfigureAwait(false);
            if (game == null)
            {
                _log.LogDebug("Patchbot game {GameName} not found", gameName);
                await SendGameNotFoundAsync(context.Channel, gameName).ConfigureAwait(false);
                return;
            }
            if (game.SubscriberIDs.Remove(context.User.Id))
                await _patchbotGamesStore.DeleteAsync(game, cancellationToken).ConfigureAwait(false);
            await context.ReplyAsync($"{_einherjiOptions.CurrentValue.SuccessSymbol} You will no longer be pinged about `{game.Name}` updates.").ConfigureAwait(false);
        }

        [RegexCommand(@"^patchbot add(?:\s(.+))?")]
        [Hidden]
        [RestrictCommand]
        private async Task CmdAddGameAsync(CommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = _log.BeginCommandScope(context, this);
            if (context.User.Id != _einherjiOptions.CurrentValue.AuthorID)
            {
                await SendInsufficientPermissionsAsync(context.Channel).ConfigureAwait(false);
                return;
            }
            // get names
            if (match.Groups.Count < 2 || match.Groups[1]?.Length < 1)
            {
                await SendNameAndAliasesRequiredAsync(context.Channel).ConfigureAwait(false);
                return;
            }
            string[] names = match.Groups[1].Value.Split(_namesSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Select(name => name.Trim()).Where(name => !string.IsNullOrWhiteSpace(name)).ToArray();
            if (names.Length == 0)
            {
                await SendNameAndAliasesRequiredAsync(context.Channel).ConfigureAwait(false);
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
            await context.ReplyAsync($"{_einherjiOptions.CurrentValue.SuccessSymbol} Game `{game.Name}` updated.").ConfigureAwait(false);
        }

        [RegexCommand(@"^patchbot\s(?:remove|del|delete)(?:\s(.+))?")]
        [Hidden]
        [RestrictCommand]
        private async Task CmdRemoveGameAsync(CommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = _log.BeginCommandScope(context, this);
            if (context.User.Id != _einherjiOptions.CurrentValue.AuthorID)
            {
                await SendInsufficientPermissionsAsync(context.Channel).ConfigureAwait(false);
                return;
            }
            if (match.Groups.Count < 2 || match.Groups[1]?.Length < 1)
            {
                await SendNameRequiredAsync(context.Channel).ConfigureAwait(false);
                return;
            }


            // check if game exists
            string gameName = match.Groups[1].Value.Trim();
            _log.LogDebug("Creating patchbot game {GameName}", gameName);
            PatchbotGame game = await _patchbotGamesStore.GetAsync(gameName, cancellationToken).ConfigureAwait(false);
            if (game == null)
            {
                _log.LogDebug("Patchbot game {GameName} not found", gameName);
                await SendGameNotFoundAsync(context.Channel, gameName).ConfigureAwait(false);
                return;
            }

            await _patchbotGamesStore.DeleteAsync(game, cancellationToken).ConfigureAwait(false);
            await context.ReplyAsync($"{_einherjiOptions.CurrentValue.SuccessSymbol} Game `{game.Name}` removed.").ConfigureAwait(false);
        }

        private Task SendInsufficientPermissionsAsync(DiscordChannel channel)
            => channel.SendMessageAsync($"{_einherjiOptions.CurrentValue.FailureSymbol} Insufficient permissions.");
        private Task SendGameNotFoundAsync(DiscordChannel channel, string gameName)
            => channel.SendMessageAsync($"{_einherjiOptions.CurrentValue.FailureSymbol} Game `{gameName}` not found!");
        private Task SendNameAndAliasesRequiredAsync(DiscordChannel channel)
            => channel.SendMessageAsync($"{_einherjiOptions.CurrentValue.FailureSymbol} Please specify game name and aliases (separated with `{_namesSeparator}`).");
        private Task SendNameRequiredAsync(DiscordChannel channel)
            => channel.SendMessageAsync($"{_einherjiOptions.CurrentValue.FailureSymbol} Please specify game name.");

        public void Dispose()
        {
            try { this._hostCts?.Cancel(); } catch { }
            try { this._hostCts?.Dispose(); } catch { }
            try { this._client.MessageCreated -= OnClientMessageReceivedAsync; } catch { }
        }
    }
}
