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

        [RegexCommand("^purge(?:\\s+(\\d+))?")]
        [Name("purge <number>")]
        [Summary("Removes last *<number>* messages. You need to have permissions to remove messages.")]
        [Priority(-28)]
        private async Task CmdPurgeAsync(SocketCommandContext message, Match match, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = _log.BeginCommandScope(message, this);
            EinherjiOptions options = _einherjiOptions.CurrentValue;

            if (!(message.Channel is SocketTextChannel channel))
            {
                await message.ReplyAsync($"{options.FailureSymbol} Sir, this command is only applicable in guild channels.", cancellationToken).ConfigureAwait(false);
                return;
            }
            IGuildUser user = await message.Guild.GetGuildUserAsync(message.User).ConfigureAwait(false);
            if (!user.GetPermissions(channel).ManageMessages)
            {
                await channel.SendMessageAsync($"{options.FailureSymbol} You can't order me to do that.", cancellationToken).ConfigureAwait(false);
                return;
            }
            IGuildUser botUser = await message.Guild.GetGuildUserAsync(message.Client.CurrentUser.Id).ConfigureAwait(false);
            if (!botUser.GetPermissions(channel).ManageMessages)
            {
                await channel.SendMessageAsync($"{options.FailureSymbol} I am missing *Manage Messages* permission in this channel.", cancellationToken).ConfigureAwait(false);
                return;
            }
            if (match.Groups.Count == 1 || match.Groups[1]?.Length < 1)
            {
                await channel.SendMessageAsync($"{options.FailureSymbol} Sir, I need a positive number of messages to take down.", cancellationToken).ConfigureAwait(false);
                return;
            }
            string countString = match.Groups[1].Value;
            if (!int.TryParse(countString, out int count))
            {
                await channel.SendMessageAsync($"{options.FailureSymbol} Sir, `{countString} is not a valid number.", cancellationToken).ConfigureAwait(false);
                return;
            }
            if (count < 0)
            {
                await channel.SendMessageAsync($"{options.FailureSymbol} Sir, how am I supposed to execute removal of {count} messages?.", cancellationToken).ConfigureAwait(false);
                return;
            }

            // start a new task to prevent deletion from blocking gateway task
            _ = Task.Run(async () =>
            {
                try
                {
                    // get last X messages
                    IEnumerable<IMessage> msgs = await channel.GetMessagesAsync(count + 1, cancellationToken).FlattenAsync().ConfigureAwait(false);
                    RestUserMessage confirmationMsg = null;
                    // bulk can only delete messages not older than 2 weeks
                    DateTimeOffset bulkMaxAge = DateTimeOffset.UtcNow - TimeSpan.FromDays(14) - TimeSpan.FromSeconds(2);
                    IEnumerable<IMessage> newerMessages = msgs.Where(msg => msg.Timestamp >= bulkMaxAge);
                    IEnumerable<IMessage> olderMessages = msgs.Except(newerMessages);
                    int olderCount = olderMessages.Count();
                    int actualCount = msgs.Count() - 1;
                    _log.LogDebug("Removing {TotalCount} messages. {BulkIncompatibleCount} messages cannot be removed in bulk", msgs.Count(), olderCount);
                    // first delete bulk-deletable
                    await channel.DeleteMessagesAsync(newerMessages, cancellationToken).ConfigureAwait(false);
                    // delete older msgs one by one
                    if (olderCount > 0)
                    {
                        await SendOrUpdateConfirmationAsync($"You are requesting deletion of {actualCount} messages, {olderCount} of which are older than 2 weeks.\n" +
                            "Deleting these messages may take a while due to Discord's rate limiting, so please be patient.").ConfigureAwait(false);
                        foreach (IMessage msg in olderMessages)
                        {
                            await Task.Delay(1500).ConfigureAwait(false);
                            await channel.DeleteMessageAsync(msg, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    await SendOrUpdateConfirmationAsync(actualCount > 0 ?
                        $"{options.SuccessSymbol} Sir, your message and {actualCount} previous message{(actualCount > 1 ? "s were" : " was")} taken down." :
                        $"{options.SuccessSymbol} Sir, I deleted your message. Specify count greater than 0 to remove more than just that.").ConfigureAwait(false);
                    await Task.Delay(6 * 1000, cancellationToken).ConfigureAwait(false);
                    await channel.DeleteMessageAsync(confirmationMsg, cancellationToken).ConfigureAwait(false);

                    async Task SendOrUpdateConfirmationAsync(string text)
                    {
                        if (confirmationMsg == null)
                            confirmationMsg = await channel.SendMessageAsync(text, cancellationToken).ConfigureAwait(false);
                        else
                            await confirmationMsg.ModifyAsync(props => props.Content = text, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) when (ex.LogAsError(_log, "Exception occured when purging messages")) { }
            }, cancellationToken).ConfigureAwait(false);
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
