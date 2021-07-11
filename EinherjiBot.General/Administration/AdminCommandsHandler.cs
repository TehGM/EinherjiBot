using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly IOptionsMonitor<CommandsOptions> _commandsOptions;
        private readonly ILogger _log;

        private readonly CancellationTokenSource _hostCts;

        public AdminCommandsHandler(DiscordSocketClient client, ILogger<AdminCommandsHandler> log,
            IOptionsMonitor<EinherjiOptions> einherjiOptions, IOptionsMonitor<CommandsOptions> commandsOptions)
        {
            this._client = client;
            this._log = log;
            this._einherjiOptions = einherjiOptions;
            this._commandsOptions = commandsOptions;
            this._hostCts = new CancellationTokenSource();

            this._client.UserLeft += OnUserLeftAsync;
        }

        [RegexCommand("^move\\s?all(?:(?: from)?\\s+(?:<#)?(\\d+)(?:>)?)?(?:(?: to)?\\s+(?:<#)?(\\d+)(?:>)?)?")]
        [Name("move all <from-id> <to-id>")]
        [Summary("Moves all users from voice channel *<from-id>* to *<to-id>*. You need to have appropiate permissions.")]
        [Priority(-29)]
        private async Task CmdMoveAllAsync(SocketCommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = _log.BeginCommandScope(context, this);
            EinherjiOptions options = _einherjiOptions.CurrentValue;

            // helper method for verifying a valid channel ID
            async Task<SocketVoiceChannel> VerifyValidChannelAsync(Group matchGroup, IGuild guild, ISocketMessageChannel responseChannel)
            {
                if (matchGroup == null || !match.Success || match.Length < 1)
                    return null;
                if (!ulong.TryParse(matchGroup.Value, out ulong id))
                {
                    await responseChannel.SendMessageAsync($"{options.FailureSymbol} `{matchGroup.Value}` is not a valid channel ID.`", cancellationToken).ConfigureAwait(false);
                    return null;
                }

                // instead of doing quick check, do a series to help user pinpoint any issue
                // find channel first
                SocketChannel channel = _client.GetChannel(id);
                if (channel == null || !(channel is SocketGuildChannel guildChannel))
                {
                    await responseChannel.SendMessageAsync($"{options.FailureSymbol} I don't know any guild channel with ID `{id}`.", cancellationToken).ConfigureAwait(false);
                    return null;
                }

                // verify channel is in guild
                if (guildChannel.Guild.Id != guild.Id)
                {
                    await responseChannel.SendMessageAsync($"{options.FailureSymbol} Channel **#{guildChannel.Name}** doesn't exist in **{guild.Name}** guild.", cancellationToken).ConfigureAwait(false);
                    return null;
                }

                // lastly make sure it is a voice channel
                if (!(guildChannel is SocketVoiceChannel voiceChannel))
                {
                    await responseChannel.SendMessageAsync($"{options.FailureSymbol} {MentionUtils.MentionChannel(id)} is not a voice channel.", cancellationToken).ConfigureAwait(false);
                    return null;
                }

                return voiceChannel;
            }

            bool CanUserConnect(IVoiceChannel channel, IGuildUser user)
                => user.GetPermissions(channel).Has(ChannelPermission.ViewChannel | ChannelPermission.Connect);
            string GetVoiceChannelMention(IVoiceChannel channel, bool isEmbed = false)
                => isEmbed ? $"[#{channel.Name}](https://discordapp.com/channels/{channel.Guild.Id}/{channel.Id})" : $"**#{channel.Name}**";

            // helper method to verify if user can move user between channels
            async Task<bool> VerifyUserCanMoveAsync(IVoiceChannel channel, IGuildUser user, ISocketMessageChannel responseChannel)
            {
                if (!CanUserConnect(channel, user))
                {
                    await responseChannel.SendMessageAsync($"{options.FailureSymbol} You don't have access to {GetVoiceChannelMention(channel)}.", cancellationToken).ConfigureAwait(false);
                    return false;
                }
                if (!user.GetPermissions(channel).MoveMembers)
                {
                    await responseChannel.SendMessageAsync($"{options.FailureSymbol} You don't have *Move Members* permission in {GetVoiceChannelMention(channel)}.", cancellationToken).ConfigureAwait(false);
                    return false;
                }
                return true;
            }

            // verify it's a guild message
            if (!(context.Channel is SocketTextChannel channel))
            {
                await context.ReplyAsync($"{options.FailureSymbol} Sir, this command is only applicable in guild channels.", cancellationToken).ConfigureAwait(false);
                return;
            }

            // verify command has proper arguments
            if (match.Groups.Count < 3)
            {
                string prefix = _commandsOptions.CurrentValue.Prefix;
                await context.ReplyAsync($"{options.FailureSymbol} Please specify __both__ channels IDs.\n***{_commandsOptions.CurrentValue.Prefix}move all from <original channel ID> to <target channel ID>***", cancellationToken).ConfigureAwait(false);
                return;
            }

            // verify channels exist
            // we can do both at once, it's okay if user gets warn about both at once, and it just simplifies the code
            SocketVoiceChannel channelFrom = await VerifyValidChannelAsync(match.Groups[1], context.Guild, context.Channel).ConfigureAwait(false);
            SocketVoiceChannel channelTo = await VerifyValidChannelAsync(match.Groups[2], context.Guild, context.Channel).ConfigureAwait(false);
            if (channelFrom == null || channelTo == null)
                return;

            // verify user can see both channels, and has move permission in both
            SocketGuildUser user = await context.Guild.GetGuildUserAsync(context.User).ConfigureAwait(false);
            _log.LogTrace("Verifying user {ID} has permission to move users between channels ({ChannelFromID}) and {ChannelToName} ({ChannelToID})", user.Id, channelFrom.Name, channelFrom.Id, channelTo.Name, channelTo.Id);
            if (!user.GuildPermissions.Administrator)
            {
                if (!await VerifyUserCanMoveAsync(channelFrom, user, channel).ConfigureAwait(false)
                    || !await VerifyUserCanMoveAsync(channelTo, user, channel).ConfigureAwait(false))
                    return;
            }

            // move the users
            SocketGuildUser[] users = channelFrom.Users.ToArray();
            string channelFromMention = GetVoiceChannelMention(channelFrom);
            string channelToMention = GetVoiceChannelMention(channelTo);
            _log.LogDebug("Moving {Count} users from channel {ChannelFromName} ({ChannelFromID}) to {ChannelToName} ({ChannelToID})", users.Length, channelFrom.Name, channelFrom.Id, channelTo.Name, channelTo.Id);
            RestUserMessage response = await context.ReplyAsync($"Moving {users.Length} user{(users.Length > 1 ? "s" : null)} from {channelFromMention} to {channelToMention}.", cancellationToken).ConfigureAwait(false);
            int errCount = 0;
            for (int i = 0; i < users.Length; i++)
            {
                try
                {
                    await users[i].ModifyAsync(props => props.Channel = channelTo, cancellationToken).ConfigureAwait(false);
                }
                catch { errCount++; }
            }
            // display confirmation
            StringBuilder builder = new StringBuilder();
            int successCount = users.Length - errCount;
            builder.AppendFormat("{0} user{3} moved from {1} to {2}.", successCount.ToString(), channelFromMention, channelToMention, successCount > 1 ? "s" : null);
            if (errCount > 0)
                builder.AppendFormat("\nFailed to move {0} user{2}. {1}", errCount.ToString(), options.FailureSymbol, errCount > 1 ? "s" : null);
            await response.ModifyAsync(props => props.Content = builder.ToString(), cancellationToken).ConfigureAwait(false);
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
            SocketGuildUser user = await message.Guild.GetGuildUserAsync(message.User).ConfigureAwait(false);
            if (!user.GetPermissions(channel).ManageMessages)
            {
                await channel.SendMessageAsync($"{options.FailureSymbol} You can't order me to do that.", cancellationToken).ConfigureAwait(false);
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
                            await channel.DeleteMessageAsync(msg, cancellationToken).ConfigureAwait(false);
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

        protected Task OnUserLeftAsync(SocketGuildUser user)
        {
            if (user.Guild.SystemChannel == null)
                return Task.CompletedTask;
            EmbedBuilder embed = new EmbedBuilder()
                .WithDescription($"**{user.Mention}** *(`{user.Username}#{user.Discriminator}`)* **has left.**")
                .WithColor((Color)System.Drawing.Color.Cyan);
            return user.Guild.SystemChannel.SendMessageAsync(null, false, embed.Build(), _hostCts.Token);
        }

        public void Dispose()
        {
            try { this._hostCts?.Cancel(); } catch { }
            try { this._hostCts?.Dispose(); } catch { }
            try { this._client.UserLeft -= OnUserLeftAsync; } catch { }
        }
    }
}
