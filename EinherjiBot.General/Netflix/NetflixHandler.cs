using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TehGM.EinherjiBot.CommandsProcessing;

namespace TehGM.EinherjiBot.Netflix
{
    [LoadRegexCommands]
    [HelpCategory("Special", -99999)]
    public class NetflixHandler
    {
        private readonly INetflixAccountStore _netflixAccountStore;
        private readonly NetflixAccountOptions _netflixAccountOptions;
        private readonly EinherjiOptions _einherjiOptions;
        private readonly ILogger _log;

        private bool IsAutoRemoving => _netflixAccountOptions?.AutoRemoveDelay > TimeSpan.Zero;

        public NetflixHandler(INetflixAccountStore netflixAccountStore, ILogger<NetflixHandler> log, IOptionsSnapshot<NetflixAccountOptions> netflixAccountOptions, IOptionsSnapshot<EinherjiOptions> einherjiOptions)
        {
            this._netflixAccountStore = netflixAccountStore;
            this._log = log;
            this._netflixAccountOptions = netflixAccountOptions.Value;
            this._einherjiOptions = einherjiOptions.Value;
        }

        [RegexCommand("^netflix (?:password|account|login)")]
        [Name("netflix account")]
        [Summary("If you're a part of our Netflix team, will provide Netflix credentials.")]
        [RestrictCommand]
        [Priority(100)]
        private async Task CmdRetrieveAccountAsync(CommandContext context, CancellationToken cancellationToken = default)
        {
            _log.LogDebug("Retrieving Netlix account credentials");
            if (context.Channel.IsPrivate)
            {
                _log.LogTrace("Aborting Netflix account credentials retrieving: Group only");
                await SendErrorAsync($"{_einherjiOptions.FailureSymbol} You can't do this in private message.\nGo to {GetAllowedChannelsMentionsText()}.", context.Channel).ConfigureAwait(false);
                return;
            }
            DiscordMember user = await context.GetGuildMemberAsync().ConfigureAwait(false);
            if (!_netflixAccountOptions.CanRetrieve(user))
            {
                _log.LogTrace("Aborting Netflix account credentials retrieving: User not privileged");
                await SendErrorAsync($"{_einherjiOptions.FailureSymbol} You need {GetAllowedRolesMentionsText()} role to do this.", context.Channel).ConfigureAwait(false);
                return;
            }
            if (!_netflixAccountOptions.IsChannelAllowed(context.Channel))
            {
                _log.LogTrace("Aborting Netflix account credentials retrieving: Wrong channel");
                await SendErrorAsync($"You can't do this here.\nGo to {GetAllowedChannelsMentionsText()}.", context.Channel).ConfigureAwait(false);
                return;
            }

            // retrieve info from store
            NetflixAccount account = await _netflixAccountStore.GetAsync(cancellationToken).ConfigureAwait(false);

            // create message
            DiscordUser modifiedByUser = null;
            if (account.ModifiedByID != null)
                modifiedByUser = await context.Client.GetUserAsync(account.ModifiedByID.Value).ConfigureAwait(false);
            DiscordEmbedBuilder embed = CreateConfirmationEmbed(account, modifiedByUser);
            string text = this.IsAutoRemoving ? GetAutoremoveText() : null;
            DiscordMessage sentMsg = await context.ReplyAsync(text, embed.Build()).ConfigureAwait(false);
            // auto remove
            if (this.IsAutoRemoving)
                RemoveMessagesDelayed(_netflixAccountOptions.AutoRemoveDelay, cancellationToken, sentMsg, context.Message);
        }

        [RegexCommand("^netflix set (login|email|username|password|pass|pwd) (.+)")]
        [Hidden]
        [RestrictCommand]
        [Priority(99)]
        private async Task CmdUpdateAccountAsync(CommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            _log.LogDebug("Updating Netlix account credentials");
            if (context.Channel.IsPrivate)
            {
                _log.LogTrace("Aborting Netflix account credentials updating: Group only");
                await SendErrorAsync($"{_einherjiOptions.FailureSymbol} You can't do this in private message.\nGo to {GetAllowedChannelsMentionsText()}.", context.Channel).ConfigureAwait(false);
                return;
            }
            DiscordMember user = await context.GetGuildMemberAsync().ConfigureAwait(false);
            if (!_netflixAccountOptions.CanModify(user))
            {
                _log.LogTrace("Aborting Netflix account credentials updating: User not privileged");
                await SendErrorAsync($"{_einherjiOptions.FailureSymbol} You need {GetAllowedRolesMentionsText()} role to do this.", context.Channel).ConfigureAwait(false);
                return;
            }
            if (!_netflixAccountOptions.IsChannelAllowed(context.Channel))
            {
                _log.LogTrace("Aborting Netflix account credentials updating: Wrong channel");
                await SendErrorAsync($"You can't do this here.\nGo to {GetAllowedChannelsMentionsText()}.", context.Channel).ConfigureAwait(false);
                return;
            }

            // retrieve info from store
            NetflixAccount account = await _netflixAccountStore.GetAsync(cancellationToken).ConfigureAwait(false);

            // update and save
            SetMode mode = StringToSetMode(match.Groups[1].Value);
            string value = match.Groups[2].Value;
            string responseText = null;
            if (mode == SetMode.Login)
            {
                account.SetLogin(value, context.User.Id);
                responseText = $"{_einherjiOptions.SuccessSymbol} You have set Netflix account login to `{value}`.";
            }
            if (mode == SetMode.Password)
            {
                account.SetPassword(value, context.User.Id);
                responseText = $"{_einherjiOptions.SuccessSymbol} You have set Netflix account password to `{value}`.";
            }

            await _netflixAccountStore.SetAsync(account, cancellationToken).ConfigureAwait(false);

            // create message
            DiscordEmbedBuilder embed = CreateConfirmationEmbed(account, context.User);
            embed.WithDescription(responseText);
            string text = this.IsAutoRemoving ? GetAutoremoveText() : null;
            DiscordMessage sentMsg = await context.ReplyAsync(text, embed.Build()).ConfigureAwait(false);
            // auto remove
            if (this.IsAutoRemoving)
                RemoveMessagesDelayed(_netflixAccountOptions.AutoRemoveDelay, cancellationToken, sentMsg, context.Message);
        }

        private string GetAutoremoveText()
            => $"I will remove this and your message in {_netflixAccountOptions.AutoRemoveDelay.ToShortFriendlyString()}.";

        private DiscordEmbedBuilder CreateConfirmationEmbed(NetflixAccount account, DiscordUser modifiedBy)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .AddField("Login", account.Login)
                .AddField("Password", account.Password)
                .WithColor(_einherjiOptions.EmbedSuccessColor)
                .WithThumbnail("https://historia.org.pl/wp-content/uploads/2018/04/netflix-logo.jpg");
            if (modifiedBy != null)
            {
                embed.WithTimestamp(account.ModifiedTimestampUTC)
                .WithFooter($"Last modified by {modifiedBy.Username}#{modifiedBy.Discriminator}", modifiedBy.GetSafeAvatarUrl());
            }
            return embed;
        }

        private Task SendErrorAsync(string text, DiscordChannel channel, string mention = null, CancellationToken cancellationToken = default)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithColor(_einherjiOptions.EmbedErrorColor)
                .WithTitle("Error")
                .WithDescription(text);
            return channel.SendMessageAsync(mention, embed.Build());
        }

        private string GetAllowedRolesMentionsText()
            => _netflixAccountOptions.RetrieveRoleIDs.Select(id => MentionID.Role(id)).JoinAsSentence(lastSeparator: " or ");

        private string GetAllowedChannelsMentionsText()
            => _netflixAccountOptions.AllowedChannelsIDs.Select(id => MentionID.Channel(id)).JoinAsSentence(lastSeparator: " or ");

        private void RemoveMessagesDelayed(TimeSpan delay, CancellationToken cancellationToken, params DiscordMessage[] messages)
        {
            if (messages.Length == 0)
                return;
            DiscordChannel channel = messages[0].Channel;
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    await channel.DeleteMessagesAsync(messages).ConfigureAwait(false);
                }
                catch (OperationCanceledException) { _log.LogWarning("Auto-removal of Netflix account message canceled"); }
                catch (Exception ex) when (ex.LogAsError(_log, "Exception occured when auto-removing Netflix account message")) { }
            }, cancellationToken).ConfigureAwait(false);
        }

        private static SetMode StringToSetMode(string value)
        {
            switch (value.ToLower())
            {
                case "login":
                case "email":
                case "username":
                    return SetMode.Login;
                case "password":
                case "pass":
                case "pwd":
                    return SetMode.Password;
                default:
                    throw new ArgumentException($"Invalid {nameof(SetMode)} \"{value.ToLower()}\"");
            }
        }

        private enum SetMode
        {
            Password,
            Login
        }
    }
}
