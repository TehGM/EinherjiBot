using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TehGM.EinherjiBot.CommandsProcessing;

namespace TehGM.EinherjiBot.Administration
{
    [LoadRegexCommands]
    [PersistentModule(PreInitialize = true)]
    class BotChannelsRedirectionHandler : IDisposable
    {
        private readonly DiscordSocketClient _client;
        private readonly IOptionsMonitor<BotChannelsRedirectionOptions> _redirectionOptions;
        private readonly IOptionsMonitor<EinherjiOptions> _einherjiOptions;
        private readonly ILogger _log;

        public BotChannelsRedirectionHandler(DiscordSocketClient client, ILogger<BotChannelsRedirectionHandler> log,
            IOptionsMonitor<BotChannelsRedirectionOptions> redirectionOptions, IOptionsMonitor<EinherjiOptions> einherjiOptions)
        {
            this._client = client;
            this._redirectionOptions = redirectionOptions;
            this._einherjiOptions = einherjiOptions;
            this._log = log;

            this._client.MessageReceived += OnClientMessageReceivedAsync;
        }

        private async Task OnClientMessageReceivedAsync(SocketMessage message)
        {
            BotChannelsRedirectionOptions options = _redirectionOptions.CurrentValue;

            // allow if channel or user is on exceptions list
            if (!(message.Channel is SocketTextChannel channel))
                return;
            if (options.IgnoredChannelIDs.Contains(message.Channel.Id))
                return;
            if (options.IgnoredUserIDs.Contains(message.Author.Id))
                return;
            if (options.IgnoreBots && (message.Author.IsBot || message.Author.IsWebhook))
                return;

            foreach (BotChannelsRedirection redirection in options.Redirections)
            {
                // allow if channel is allowed
                if (redirection.AllowedChannelIDs.Contains(message.Channel.Id))
                    continue;

                // if channel isn't on allowance list, let's check if it's a bot command
                foreach (Regex regex in redirection.GetRegexes())
                {
                    // skip regex if not matching
                    if (!regex.IsMatch(message.Content))
                        continue;

                    // if regex matched, means that the bot should not be used in this channel - tell the user off!

                    SocketGuildUser user = await channel.Guild.GetGuildUserAsync(message.Author).ConfigureAwait(false);
                    string channelsText = GetChannelsMentionsText(redirection.AllowedChannelIDs, user);
                    if (channelsText == null)
                        return;
                    await message.ReplyAsync($"{_einherjiOptions.CurrentValue.FailureSymbol} {user.Mention}, please go to {channelsText} to use {GetUsersMentionsText(redirection.BotIDs)}.");
                }
            }
        }


        private static string GetChannelsMentionsText(IEnumerable<ulong> ids, SocketGuildUser user)
            => ids.Where(id => user.Guild.GetChannel(id) != null
                && user.GetPermissions(user.Guild.GetChannel(id)).Has(ChannelPermission.ViewChannel | ChannelPermission.SendMessages))
                .Select(id => MentionUtils.MentionChannel(id)).JoinAsSentence(", ", " or ");

        private static string GetUsersMentionsText(IEnumerable<ulong> ids)
        {
            if (ids?.Any() != true)
                return "this bot";
            return ids.Select(id => MentionUtils.MentionUser(id)).JoinAsSentence(", ", " and ");
        }

        public void Dispose()
        {
            try { this._client.MessageReceived -= OnClientMessageReceivedAsync; } catch { }
        }
    }
}
