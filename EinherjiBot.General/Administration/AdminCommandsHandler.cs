using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TehGM.EinherjiBot.CommandsProcessing;

namespace TehGM.EinherjiBot.Administration
{
    [RegexCommandsModule(IsPersistent = true, PreInitialize = true)]
    [HelpCategory("Special", -99999)]
    public class AdminCommandsHandler : IDisposable
    {
        private readonly DiscordClient _client;
        private readonly IOptionsMonitor<EinherjiOptions> _einherjiOptions;
        private readonly ILogger _log;

        private readonly CancellationTokenSource _hostCts;

        public AdminCommandsHandler(DiscordClient client, ILogger<AdminCommandsHandler> log,
            IOptionsMonitor<EinherjiOptions> einherjiOptions)
        {
            this._client = client;
            this._log = log;
            this._einherjiOptions = einherjiOptions;
            this._hostCts = new CancellationTokenSource();

            this._client.GuildMemberRemoved += OnUserLeftAsync;
        }

        [RegexCommand("^purge(?:\\s+(\\d+))?")]
        [Name("purge <number>")]
        [Summary("Removes last *<number>* messages. You need to have permissions to remove messages.")]
        [Priority(-28)]
        private async Task CmdPurgeAsync(CommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            EinherjiOptions options = _einherjiOptions.CurrentValue;

            if (!context.Channel.IsGuildText())
            {
                await context.ReplyAsync($"{options.FailureSymbol} Sir, this command is only applicable in guild channels.").ConfigureAwait(false);
                return;
            }
            DiscordMember user = await context.GetGuildMemberAsync().ConfigureAwait(false);
            if (!user.HasChannelPermissions(context.Channel, Permissions.ManageMessages))
            {
                await context.ReplyAsync($"{options.FailureSymbol} You can't order me to do that.").ConfigureAwait(false);
                return;
            }
            DiscordMember botUser = await context.GetGuildMemberAsync().ConfigureAwait(false);
            if (!botUser.HasChannelPermissions(context.Channel, Permissions.ManageMessages))
            {
                await context.ReplyAsync($"{options.FailureSymbol} I am missing *Manage Messages* permission in this channel.").ConfigureAwait(false);
                return;
            }
            if (match.Groups.Count == 1 || match.Groups[1]?.Length < 1)
            {
                await context.ReplyAsync($"{options.FailureSymbol} Sir, I need a positive number of messages to take down.").ConfigureAwait(false);
                return;
            }
            string countString = match.Groups[1].Value;
            if (!int.TryParse(countString, out int count))
            {
                await context.ReplyAsync($"{options.FailureSymbol} Sir, `{countString} is not a valid number.").ConfigureAwait(false);
                return;
            }
            if (count < 0)
            {
                await context.ReplyAsync($"{options.FailureSymbol} Sir, how am I supposed to execute removal of {count} messages?.").ConfigureAwait(false);
                return;
            }

            // start a new task to prevent deletion from blocking gateway task
            _ = Task.Run(async () =>
            {
                try
                {
                    // get last X messages
                    IEnumerable<DiscordMessage> msgs = await context.Channel.GetMessagesAsync(count + 1).ConfigureAwait(false);
                    DiscordMessage confirmationMsg = null;
                    // bulk can only delete messages not older than 2 weeks
                    DateTimeOffset bulkMaxAge = DateTimeOffset.UtcNow - TimeSpan.FromDays(14) - TimeSpan.FromSeconds(2);
                    IEnumerable<DiscordMessage> newerMessages = msgs.Where(msg => msg.Timestamp >= bulkMaxAge);
                    IEnumerable<DiscordMessage> olderMessages = msgs.Except(newerMessages);
                    int olderCount = olderMessages.Count();
                    int actualCount = msgs.Count() - 1;
                    _log.LogDebug("Removing {TotalCount} messages. {BulkIncompatibleCount} messages cannot be removed in bulk", msgs.Count(), olderCount);
                    // first delete bulk-deletable
                    await context.Channel.DeleteMessagesAsync(newerMessages).ConfigureAwait(false);
                    // delete older msgs one by one
                    if (olderCount > 0)
                    {
                        await SendOrUpdateConfirmationAsync($"You are requesting deletion of {actualCount} messages, {olderCount} of which are older than 2 weeks.\n" +
                            "Deleting these messages may take a while due to Discord's rate limiting, so please be patient.").ConfigureAwait(false);
                        foreach (DiscordMessage msg in olderMessages)
                        {
                            await Task.Delay(1500).ConfigureAwait(false);
                            await context.Channel.DeleteMessageAsync(msg).ConfigureAwait(false);
                        }
                    }
                    await SendOrUpdateConfirmationAsync(actualCount > 0 ?
                        $"{options.SuccessSymbol} Sir, your message and {actualCount} previous message{(actualCount > 1 ? "s were" : " was")} taken down." :
                        $"{options.SuccessSymbol} Sir, I deleted your message. Specify count greater than 0 to remove more than just that.").ConfigureAwait(false);
                    await Task.Delay(6 * 1000, cancellationToken).ConfigureAwait(false);
                    await context.Channel.DeleteMessageAsync(confirmationMsg).ConfigureAwait(false);

                    async Task SendOrUpdateConfirmationAsync(string text)
                    {
                        if (confirmationMsg == null)
                            confirmationMsg = await context.ReplyAsync(text).ConfigureAwait(false);
                        else
                            await confirmationMsg.ModifyAsync(text).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) when (ex.LogAsError(_log, "Exception occured when purging messages")) { }
            }, cancellationToken).ConfigureAwait(false);
        }

        protected Task OnUserLeftAsync(DiscordClient client, GuildMemberRemoveEventArgs e)
        {
            if (e.Guild.SystemChannel == null)
                return Task.CompletedTask;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithDescription($"**{e.Member.Mention}** *(`{e.Member.Username}#{e.Member.Discriminator}`)* **has left.**")
                .WithColor(Color.Cyan);
            return e.Guild.SystemChannel.SendMessageAsync(embed);
        }

        public void Dispose()
        {
            try { this._hostCts?.Cancel(); } catch { }
            try { this._hostCts?.Dispose(); } catch { }
            try { this._client.GuildMemberRemoved -= OnUserLeftAsync; } catch { }
        }
    }
}
