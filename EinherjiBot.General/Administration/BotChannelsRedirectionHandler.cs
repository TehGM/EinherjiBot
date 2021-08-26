using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TehGM.EinherjiBot.Administration
{
    class BotChannelsRedirectionHandler : IHostedService, IDisposable
    {
        private readonly DiscordClient _client;
        private readonly IOptionsMonitor<BotChannelsRedirectionOptions> _redirectionOptions;
        private readonly IOptionsMonitor<EinherjiOptions> _einherjiOptions;
        private readonly ILogger _log;
        private readonly CancellationTokenSource _hostCts;

        public BotChannelsRedirectionHandler(DiscordClient client, ILogger<BotChannelsRedirectionHandler> log,
            IOptionsMonitor<BotChannelsRedirectionOptions> redirectionOptions, IOptionsMonitor<EinherjiOptions> einherjiOptions)
        {
            this._client = client;
            this._redirectionOptions = redirectionOptions;
            this._einherjiOptions = einherjiOptions;
            this._log = log;
            this._hostCts = new CancellationTokenSource();

            this._client.MessageCreated += OnClientMessageReceivedAsync;
        }

        private async Task OnClientMessageReceivedAsync(DiscordClient client, MessageCreateEventArgs e)
        {
            BotChannelsRedirectionOptions options = _redirectionOptions.CurrentValue;

            // allow if channel or user is on exceptions list
            if (!e.Channel.IsText())
                return;
            if (options.IgnoredChannelIDs?.Contains(e.Channel.Id) == true)
                return;
            if (options.IgnoredUserIDs?.Contains(e.Author.Id) == true)
                return;
            if (options.IgnoreBots && e.Author.IsBot)
                return;

            foreach (BotChannelsRedirection redirection in options.Redirections)
            {
                // allow if channel is allowed
                if (redirection.AllowedChannelIDs.Contains(e.Channel.Id))
                    continue;

                // if channel isn't on allowance list, let's check if it's a bot command
                foreach (Regex regex in redirection.GetRegexes())
                {
                    // skip regex if not matching
                    if (!regex.IsMatch(e.Message.Content))
                        continue;

                    // if regex matched, means that the bot should not be used in this channel - tell the user off!
                    DiscordMember user = await e.Guild.GetMemberAsync(e.Author.Id).ConfigureAwait(false);
                    string channelsText = GetChannelsMentionsText(redirection.AllowedChannelIDs, user);
                    if (channelsText == null)
                        return;
                    await e.Channel.SendMessageAsync($"{_einherjiOptions.CurrentValue.FailureSymbol} {user.Mention}, please go to {channelsText} to use {GetUsersMentionsText(redirection.BotIDs)}.").ConfigureAwait(false);
                }
            }
        }

        private static string GetChannelsMentionsText(IEnumerable<ulong> ids, DiscordMember user)
            => ids.Select(id => user.Guild.GetChannel(id))
                .Where(ch => ch != null && user.HasChannelPermissions(ch, Permissions.SendMessages | Permissions.AccessChannels))
                .Select(ch => Formatter.Mention(ch))
                .JoinAsSentence(", ", " or ");

        private static string GetUsersMentionsText(IEnumerable<ulong> ids)
        {
            if (ids?.Any() != true)
                return "this bot";
            return ids.Select(id => new UserMention(id)).JoinAsSentence(", ", " and ");
        }

        public void Dispose()
        {
            try { this._hostCts?.Cancel(); } catch { }
            try { this._hostCts?.Dispose(); } catch { }
            try { this._client.MessageCreated -= OnClientMessageReceivedAsync; } catch { }
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
