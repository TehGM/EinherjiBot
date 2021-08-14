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
    [HelpCategory("Voice Channels Admin", -900)]
    public class VoiceChannelsCommandsHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly IOptionsMonitor<EinherjiOptions> _einherjiOptions;
        private readonly IOptionsMonitor<CommandsOptions> _commandsOptions;
        private readonly ILogger _log;

        public VoiceChannelsCommandsHandler(DiscordSocketClient client, ILogger<VoiceChannelsCommandsHandler> log,
            IOptionsMonitor<EinherjiOptions> einherjiOptions, IOptionsMonitor<CommandsOptions> commandsOptions)
        {
            this._client = client;
            this._log = log;
            this._einherjiOptions = einherjiOptions;
            this._commandsOptions = commandsOptions;
        }

        [RegexCommand("^move\\s?all(?:(?: from)?\\s+(?:<#)?(\\d+)(?:>)?)?(?:(?: to)?\\s+(?:<#)?(\\d+)(?:>)?)?")]
        [Name("move all <from-id> <to-id>")]
        [Summary("Moves all users from voice channel *<from-id>* to *<to-id>*. You need to have appropiate permissions.")]
        [Priority(-29)]
        private async Task CmdMoveAllAsync(SocketCommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = _log.BeginCommandScope(context, this);
            EinherjiOptions options = _einherjiOptions.CurrentValue;

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
            SocketVoiceChannel channelFrom = await VerifyValidChannelAsync(match.Groups[1], context.Guild, context.Channel, cancellationToken).ConfigureAwait(false);
            SocketVoiceChannel channelTo = await VerifyValidChannelAsync(match.Groups[2], context.Guild, context.Channel, cancellationToken).ConfigureAwait(false);
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



        // helper method for verifying a valid channel ID
        private async Task<SocketVoiceChannel> VerifyValidChannelAsync(Group matchGroup, IGuild guild, ISocketMessageChannel responseChannel, CancellationToken cancellationToken)
        {
            if (matchGroup == null || !matchGroup.Success || matchGroup.Length < 1)
                return null;

            EinherjiOptions options = _einherjiOptions.CurrentValue;
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
    }
}
