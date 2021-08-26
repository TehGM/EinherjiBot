using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TehGM.EinherjiBot.CommandsProcessing;

namespace TehGM.EinherjiBot.Administration
{
    [LoadRegexCommands]
    [HelpCategory("Voice Channels Admin", -900)]
    public class VoiceChannelsCommandsHandler
    {
        private readonly DiscordClient _client;
        private readonly IOptionsMonitor<EinherjiOptions> _einherjiOptions;
        private readonly IOptionsMonitor<CommandsOptions> _commandsOptions;
        private readonly ILogger _log;

        public VoiceChannelsCommandsHandler(DiscordClient client, ILogger<VoiceChannelsCommandsHandler> log,
            IOptionsMonitor<EinherjiOptions> einherjiOptions, IOptionsMonitor<CommandsOptions> commandsOptions)
        {
            this._client = client;
            this._log = log;
            this._einherjiOptions = einherjiOptions;
            this._commandsOptions = commandsOptions;
        }

        [RegexCommand(@"^mute\s?all(?:(?: in)?\s+(?:<#)?(\d+)(?:>)?)?")]
        [Name("mute all [<channel-id>]")]
        [Summary("Mutes all users in a voice channel (either by ID, or the one you're currently in). You need to have appropiate permissions.")]
        [Priority(-25)]
        private async Task CmdMuteAllAsync(CommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            VoiceChannelMatch channelMatch = await this.MatchChannelAsync(context, match, 1).ConfigureAwait(false);
            VoiceChannelAction action = new VoiceChannelAction(user => user.Muted = true, Permissions.DeafenMembers, "Muting", "muted", "mute");

            await PerformActionOnAllAsync(context, channelMatch, action).ConfigureAwait(false);
        }

        [RegexCommand(@"^unmute\s?all(?:(?: in)?\s+(?:<#)?(\d+)(?:>)?)?")]
        [Name("unmute all [<channel-id>]")]
        [Summary("Unmutes all users in a voice channel (either by ID, or the one you're currently in). You need to have appropiate permissions.")]
        [Priority(-26)]
        private async Task CmdUnmuteAllAsync(CommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            VoiceChannelMatch channelMatch = await this.MatchChannelAsync(context, match, 1).ConfigureAwait(false);
            VoiceChannelAction action = new VoiceChannelAction(user => user.Muted = false, Permissions.DeafenMembers, "Unmuting", "unmuted", "unmute");

            await PerformActionOnAllAsync(context, channelMatch, action).ConfigureAwait(false);
        }

        [RegexCommand(@"^deafen\s?all(?:(?: in)?\s+(?:<#)?(\d+)(?:>)?)?")]
        [Name("deafen all [<channel-id>]")]
        [Summary("Deafens all users in a voice channel (either by ID, or the one you're currently in). You need to have appropiate permissions.")]
        [Priority(-27)]
        private async Task CmdDeafenAllAsync(CommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            VoiceChannelMatch channelMatch = await this.MatchChannelAsync(context, match, 1).ConfigureAwait(false);
            VoiceChannelAction action = new VoiceChannelAction(user => user.Deafened = true, Permissions.DeafenMembers, "Deafening", "deafened", "deafen");

            await PerformActionOnAllAsync(context, channelMatch, action).ConfigureAwait(false);
        }

        [RegexCommand(@"^undeafen\s?all(?:(?: in)?\s+(?:<#)?(\d+)(?:>)?)?")]
        [Name("undeafen all [<channel-id>]")]
        [Summary("Undeafens all users in a voice channel (either by ID, or the one you're currently in). You need to have appropiate permissions.")]
        [Priority(-28)]
        private async Task CmdUndeafenAllAsync(CommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            VoiceChannelMatch channelMatch = await this.MatchChannelAsync(context, match, 1).ConfigureAwait(false);
            VoiceChannelAction action = new VoiceChannelAction(user => user.Deafened = false, Permissions.DeafenMembers, "Undeafening", "undeafened", "undeafen");

            await PerformActionOnAllAsync(context, channelMatch, action).ConfigureAwait(false);
        }

        private async Task PerformActionOnAllAsync(CommandContext context, VoiceChannelMatch channelMatch, VoiceChannelAction action)
        {
            if (!channelMatch.IsSuccess)
                return;

            // verify it's a guild message
            DiscordChannel responseChannel = await this.VerifyGuildChannelAsync(context).ConfigureAwait(false);
            if (responseChannel == null)
                return;

            // verify user's permissions
            _log.LogTrace("Verifying user {ID} has {Permission} permission in channel {ChannelName} ({ChannelFromID})", channelMatch.User.Id, action.RequiredPermission, channelMatch.Channel.Name, channelMatch.Channel.Id);
            if (!await this.VerifyUserPermissionsAsync(context, channelMatch.Channel, channelMatch.User.Id, action.RequiredPermission).ConfigureAwait(false))
                return;

            // verify bot's permissions
            _log.LogTrace("Verifying the bot has {Permission} permission in channel {ChannelName} ({ChannelFromID})", action.RequiredPermission, channelMatch.Channel.Name, channelMatch.Channel.Id);
            if (!await this.VerifyUserPermissionsAsync(context, channelMatch.Channel, this._client.CurrentUser.Id, action.RequiredPermission).ConfigureAwait(false))
                return;

            // perform the action on the users
            DiscordMember[] users = channelMatch.Channel.Users.ToArray();
            string channelMention = GetVoiceChannelMention(channelMatch.Channel);
            _log.LogDebug($"{action.ModeWord} {{Count}} users from channel {{ChannelName}} ({{ChannelID}})", users.Length, channelMatch.Channel.Name, channelMatch.Channel.Id);
            DiscordMessage response = await context.ReplyAsync($"{action.ModeWord} {users.Length} user{(users.Length > 1 ? "s" : null)} in {channelMention}.").ConfigureAwait(false);
            int errorCount = 0;
            foreach (DiscordMember user in users)
            {
                try
                {
                    await user.ModifyAsync(action.Action).ConfigureAwait(false);
                }
                catch { errorCount++; }
            }
            // display confirmation
            StringBuilder builder = new StringBuilder();
            int successCount = users.Length - errorCount;
            builder.AppendFormat("{0} user{1} {2} in {3}.", successCount.ToString(), successCount > 1 || successCount == 0 ? "s" : null, action.ActionCompletedWord, channelMention);
            if (errorCount > 0)
                builder.AppendFormat("\nFailed to {3} {0} user{2}. {1}", errorCount.ToString(), this._einherjiOptions.CurrentValue.FailureSymbol, errorCount > 1 ? "s" : null, action.ActionFailedWord);
            await response.ModifyAsync(builder.ToString()).ConfigureAwait(false);
        }

        [RegexCommand("^move\\s?all(?:(?: from)?\\s+(?:<#)?(\\d+)(?:>)?)?(?:(?: to)?\\s+(?:<#)?(\\d+)(?:>)?)?")]
        [Name("move all <from-id> <to-id>")]
        [Summary("Moves all users from voice channel *<from-id>* to *<to-id>*. You need to have appropiate permissions.")]
        [Priority(-29)]
        private async Task CmdMoveAllAsync(CommandContext context, Match match)
        {
            EinherjiOptions options = _einherjiOptions.CurrentValue;

            // verify it's a guild message
            DiscordChannel channel = await this.VerifyGuildChannelAsync(context).ConfigureAwait(false);
            if (channel == null)
                return;

            // verify command has proper arguments
            if (match.Groups.Count < 3)
            {
                string prefix = _commandsOptions.CurrentValue.Prefix;
                await context.ReplyAsync($"{options.FailureSymbol} Please specify __both__ channels IDs.\n***{_commandsOptions.CurrentValue.Prefix}move all from <original channel ID> to <target channel ID>***").ConfigureAwait(false);
                return;
            }

            // verify channels exist
            // we can do both at once, it's okay if user gets warn about both at once, and it just simplifies the code
            DiscordChannel channelFrom = await VerifyValidVoiceChannelAsync(match.Groups[1], context.Guild, context.Channel).ConfigureAwait(false);
            DiscordChannel channelTo = await VerifyValidVoiceChannelAsync(match.Groups[2], context.Guild, context.Channel).ConfigureAwait(false);
            if (channelFrom == null || channelTo == null)
                return;

            // verify user can see both channels, and has move permission in both
            _log.LogTrace("Verifying user {ID} has permission to move users between channels {ChannelFromName} ({ChannelFromID}) and {ChannelToName} ({ChannelToID})", 
                context.User.Id, channelFrom.Name, channelFrom.Id, channelTo.Name, channelTo.Id);
            if (!await this.VerifyUserPermissionsAsync(context, channelFrom, context.User.Id, Permissions.MoveMembers).ConfigureAwait(false) || 
                !await this.VerifyUserPermissionsAsync(context, channelTo, context.User.Id, Permissions.MoveMembers).ConfigureAwait(false))
                return;

            // verify bot also has permissions
            _log.LogTrace("Verifying the bot has permission to move users between channels {ChannelFromName} ({ChannelFromID}) and {ChannelToName} ({ChannelToID})", 
                channelFrom.Name, channelFrom.Id, channelTo.Name, channelTo.Id);
            if (!await this.VerifyUserPermissionsAsync(context, channelFrom, _client.CurrentUser.Id, Permissions.MoveMembers).ConfigureAwait(false) ||
                !await this.VerifyUserPermissionsAsync(context, channelTo, _client.CurrentUser.Id, Permissions.MoveMembers).ConfigureAwait(false))
                return;

            // move the users
            DiscordMember[] users = channelFrom.Users.ToArray();
            string channelFromMention = GetVoiceChannelMention(channelFrom);
            string channelToMention = GetVoiceChannelMention(channelTo);
            _log.LogDebug("Moving {Count} users from channel {ChannelFromName} ({ChannelFromID}) to {ChannelToName} ({ChannelToID})", users.Length, channelFrom.Name, channelFrom.Id, channelTo.Name, channelTo.Id);
            DiscordMessage response = await context.ReplyAsync($"Moving {users.Length} user{(users.Length > 1 ? "s" : null)} from {channelFromMention} to {channelToMention}.").ConfigureAwait(false);
            int errorCount = 0;
            foreach (DiscordMember user in users)
            {
                try
                {
                    await user.ModifyAsync(props => props.VoiceChannel = channelTo).ConfigureAwait(false);
                }
                catch { errorCount++; }
            }
            // display confirmation
            StringBuilder builder = new StringBuilder();
            int successCount = users.Length - errorCount;
            builder.AppendFormat("{0} user{3} moved from {1} to {2}.", successCount.ToString(), channelFromMention, channelToMention, successCount > 1 || successCount == 0 ? "s" : null);
            if (errorCount > 0)
                builder.AppendFormat("\nFailed to move {0} user{2}. {1}", errorCount.ToString(), options.FailureSymbol, errorCount > 1 ? "s" : null);
            await response.ModifyAsync(builder.ToString()).ConfigureAwait(false);
        }

        // helper method for verifying a valid channel ID
        private async Task<DiscordChannel> VerifyValidVoiceChannelAsync(Group matchGroup, DiscordGuild guild, DiscordChannel responseChannel)
        {
            if (matchGroup == null || !matchGroup.Success || matchGroup.Length < 1)
                return null;

            EinherjiOptions options = _einherjiOptions.CurrentValue;
            if (!ulong.TryParse(matchGroup.Value, out ulong id))
            {
                await responseChannel.SendMessageAsync($"{options.FailureSymbol} `{matchGroup.Value}` is not a valid channel ID.`").ConfigureAwait(false);
                return null;
            }

            // instead of doing quick check, do a series to help user pinpoint any issue
            // find channel first
            DiscordChannel channel = guild.GetChannel(id);
            if (channel == null)
            {
                await responseChannel.SendMessageAsync($"{options.FailureSymbol} I don't know any guild channel with ID `{id}`.").ConfigureAwait(false);
                return null;
            }

            // lastly make sure it is a voice channel
            if (channel.Type != ChannelType.Voice)
            {
                await responseChannel.SendMessageAsync($"{options.FailureSymbol} {Formatter.Mention(channel)} is not a voice channel.").ConfigureAwait(false);
                return null;
            }

            return channel;
        }

        private async Task<bool> VerifyUserPermissionsAsync(CommandContext context, DiscordChannel channel, ulong userID, Permissions permission)
        {
            DiscordMember user = await channel.Guild.GetMemberAsync(userID).ConfigureAwait(false);

            if (user.HasPermissions(Permissions.Administrator))
                return true;

            bool isSelf = userID == _client.CurrentUser.Id;
            string memberName = isSelf ? "I" : "You";
            if (!user.HasChannelPermissions(channel, Permissions.AccessChannels | Permissions.UseVoice))
            {
                await context.InlineReplyAsync($"{this._einherjiOptions.CurrentValue.FailureSymbol} {memberName} don't have access to {GetVoiceChannelMention(channel)}.").ConfigureAwait(false);
                return false;
            }
            if (!user.HasChannelPermissions(channel, permission))
            {
                await context.InlineReplyAsync($"{this._einherjiOptions.CurrentValue.FailureSymbol} {memberName} don't have *{GetPermissionDisplayName(permission)}* permission in {GetVoiceChannelMention(channel)}.").ConfigureAwait(false);
                return false;
            }
            return true;
        }

        private async Task<DiscordChannel> VerifyGuildChannelAsync(CommandContext context)
        {
            if (!context.Channel.IsGuildText())
            {
                await context.InlineReplyAsync($"{this._einherjiOptions.CurrentValue.FailureSymbol} Sir, this command is only applicable in guild channels.").ConfigureAwait(false);
                return null;
            }
            return context.Channel;
        }

        private static string GetVoiceChannelMention(DiscordChannel channel, bool isEmbed = false)
            => isEmbed ? $"[#{channel.Name}](https://discordapp.com/channels/{channel.Guild.Id}/{channel.Id})" : $"**#{channel.Name}**";

        private async Task<VoiceChannelMatch> MatchChannelAsync(CommandContext context, Match match, int groupIndex)
        {
            DiscordMember user = await context.GetGuildMemberAsync().ConfigureAwait(false);
            DiscordChannel targetChannel;
            if (match.Groups.Count >= groupIndex + 1 && match.Groups[groupIndex].Success)
                targetChannel = await this.VerifyValidVoiceChannelAsync(match.Groups[groupIndex], context.Guild, context.Channel).ConfigureAwait(false);
            else
                targetChannel = user.VoiceState?.Channel;
            if (targetChannel == null)
            {
                await context.InlineReplyAsync($"{this._einherjiOptions.CurrentValue.FailureSymbol} You need to either provide a voice channel ID, or be in a voice channel currently. Also ensure I have access to that voice channel.").ConfigureAwait(false);
            }
            return new VoiceChannelMatch(targetChannel, user);
        }

        private static string GetPermissionDisplayName(Permissions permission)
        {
            switch (permission)
            {
                case Permissions.DeafenMembers:
                    return "Deafen Members";
                case Permissions.MoveMembers:
                    return "Move Members";
                case Permissions.MuteMembers:
                    return "Mute Members";
                default:
                    return permission.ToString();
            }
        }

        private class VoiceChannelMatch
        {
            public DiscordChannel Channel { get; }
            public bool IsSuccess => this.Channel != null;
            public DiscordMember User { get; }

            public VoiceChannelMatch(DiscordChannel channel, DiscordMember user)
            {
                if (channel.Type != ChannelType.Voice)
                    throw new ArgumentException("Channel needs to be a voice channel", nameof(channel));
                this.Channel = channel;
                this.User = user;
            }
        }

        private class VoiceChannelAction
        {
            public string ModeWord { get; init; }
            public string ActionCompletedWord { get; init; }
            public string ActionFailedWord { get; init; }
            public Action<MemberEditModel> Action { get; }
            public Permissions RequiredPermission { get; }

            public VoiceChannelAction(Action<MemberEditModel> action, Permissions requiredPermission, string modeWord, string completeWord, string failWord)
            {
                this.Action = action;
                this.RequiredPermission = requiredPermission;
                this.ModeWord = modeWord;
                this.ActionCompletedWord = completeWord;
                this.ActionFailedWord = failWord;
            }
        }
    }
}
