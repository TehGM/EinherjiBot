using System;
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

        [RegexCommand(@"^mute\s?all(?:(?: in)?\s+(?:<#)?(\d+)(?:>)?)?")]
        [Name("mute all [<channel-id>]")]
        [Summary("Mutes all users in a voice channel (either by ID, or the one you're currently in). You need to have appropiate permissions.")]
        [Priority(-25)]
        private async Task CmdMuteAllAsync(SocketCommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = _log.BeginCommandScope(context, this);

            VoiceChannelMatch channelMatch = await this.MatchChannelAsync(context, match, 1, cancellationToken).ConfigureAwait(false);
            VoiceChannelAction action = new VoiceChannelAction(user => user.Mute = true, ChannelPermission.DeafenMembers, "Muting", "muted", "mute");

            await PerformActionOnAllAsync(context, channelMatch, action, cancellationToken).ConfigureAwait(false);
        }

        [RegexCommand(@"^unmute\s?all(?:(?: in)?\s+(?:<#)?(\d+)(?:>)?)?")]
        [Name("unmute all [<channel-id>]")]
        [Summary("Unmutes all users in a voice channel (either by ID, or the one you're currently in). You need to have appropiate permissions.")]
        [Priority(-26)]
        private async Task CmdUnmuteAllAsync(SocketCommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = _log.BeginCommandScope(context, this);

            VoiceChannelMatch channelMatch = await this.MatchChannelAsync(context, match, 1, cancellationToken).ConfigureAwait(false);
            VoiceChannelAction action = new VoiceChannelAction(user => user.Mute = false, ChannelPermission.DeafenMembers, "Unmuting", "unmuted", "unmute");

            await PerformActionOnAllAsync(context, channelMatch, action, cancellationToken).ConfigureAwait(false);
        }

        [RegexCommand(@"^deafen\s?all(?:(?: in)?\s+(?:<#)?(\d+)(?:>)?)?")]
        [Name("deafen all [<channel-id>]")]
        [Summary("Deafens all users in a voice channel (either by ID, or the one you're currently in). You need to have appropiate permissions.")]
        [Priority(-27)]
        private async Task CmdDeafenAllAsync(SocketCommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = _log.BeginCommandScope(context, this);

            VoiceChannelMatch channelMatch = await this.MatchChannelAsync(context, match, 1, cancellationToken).ConfigureAwait(false);
            VoiceChannelAction action = new VoiceChannelAction(user => user.Deaf = true, ChannelPermission.DeafenMembers, "Deafening", "deafened", "deafen");

            await PerformActionOnAllAsync(context, channelMatch, action, cancellationToken).ConfigureAwait(false);
        }

        [RegexCommand(@"^undeafen\s?all(?:(?: in)?\s+(?:<#)?(\d+)(?:>)?)?")]
        [Name("undeafen all [<channel-id>]")]
        [Summary("Undeafens all users in a voice channel (either by ID, or the one you're currently in). You need to have appropiate permissions.")]
        [Priority(-28)]
        private async Task CmdUndeafenAllAsync(SocketCommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = _log.BeginCommandScope(context, this);

            VoiceChannelMatch channelMatch = await this.MatchChannelAsync(context, match, 1, cancellationToken).ConfigureAwait(false);
            VoiceChannelAction action = new VoiceChannelAction(user => user.Deaf = false, ChannelPermission.DeafenMembers, "Undeafening", "undeafened", "undeafen");

            await PerformActionOnAllAsync(context, channelMatch, action, cancellationToken).ConfigureAwait(false);
        }

        private async Task PerformActionOnAllAsync(SocketCommandContext context, VoiceChannelMatch channelMatch, VoiceChannelAction action, CancellationToken cancellationToken)
        {
            EinherjiOptions options = _einherjiOptions.CurrentValue;

            if (!channelMatch.IsSuccess)
                return;

            // verify it's a guild message
            SocketTextChannel responseChannel = await this.VerifyGuildChannelAsync(context, cancellationToken).ConfigureAwait(false);
            if (responseChannel == null)
                return;

            // verify permissions
            _log.LogTrace("Verifying user {ID} has {Permission} permission in channel {ChannelName} ({ChannelFromID})", channelMatch.User.Id, action.RequiredPermission, channelMatch.Channel.Name, channelMatch.Channel.Id);
            if (!await this.VerifyUserCanConnectAsync(channelMatch.Channel, channelMatch.User, responseChannel, cancellationToken).ConfigureAwait(false))
                return;
            if (!channelMatch.User.GuildPermissions.Administrator && !channelMatch.User.GetPermissions(channelMatch.Channel).Has(action.RequiredPermission))
            {
                await responseChannel.SendMessageAsync($"{options.FailureSymbol} You don't have *{action.PermissionDisplayName}* permission in {GetVoiceChannelMention(channelMatch.Channel)}.", cancellationToken).ConfigureAwait(false);
                return;
            }


            // perform the action on the users
            SocketGuildUser[] users = channelMatch.Channel.Users.ToArray();
            string channelMention = GetVoiceChannelMention(channelMatch.Channel);
            _log.LogDebug($"{action.ModeWord} {{Count}} users from channel {{ChannelName}} ({{ChannelID}})", users.Length, channelMatch.Channel.Name, channelMatch.Channel.Id);
            RestUserMessage response = await context.ReplyAsync($"{action.ModeWord} {users.Length} user{(users.Length > 1 ? "s" : null)} in {channelMention}.", cancellationToken).ConfigureAwait(false);
            int errCount = 0;
            for (int i = 0; i < users.Length; i++)
            {
                try
                {
                    await users[i].ModifyAsync(action.Action, cancellationToken).ConfigureAwait(false);
                }
                catch { errCount++; }
            }
            // display confirmation
            StringBuilder builder = new StringBuilder();
            int successCount = users.Length - errCount;
            builder.AppendFormat("{0} user{1} {2} in {3}.", successCount.ToString(), successCount > 1 || successCount == 0 ? "s" : null, action.ActionCompletedWord, channelMention);
            if (errCount > 0)
                builder.AppendFormat("\nFailed to {3} {0} user{2}. {1} Please make sure I have correct permissions!", errCount.ToString(), options.FailureSymbol, errCount > 1 ? "s" : null, action.ActionFailedWord);
            await response.ModifyAsync(props => props.Content = builder.ToString(), cancellationToken).ConfigureAwait(false);
        }

        [RegexCommand("^move\\s?all(?:(?: from)?\\s+(?:<#)?(\\d+)(?:>)?)?(?:(?: to)?\\s+(?:<#)?(\\d+)(?:>)?)?")]
        [Name("move all <from-id> <to-id>")]
        [Summary("Moves all users from voice channel *<from-id>* to *<to-id>*. You need to have appropiate permissions.")]
        [Priority(-29)]
        private async Task CmdMoveAllAsync(SocketCommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = _log.BeginCommandScope(context, this);
            EinherjiOptions options = _einherjiOptions.CurrentValue;

            // helper method to verify if user can move user between channels
            async Task<bool> VerifyUserCanMoveAsync(IVoiceChannel channel, IGuildUser user, ISocketMessageChannel responseChannel)
            {
                if (!await VerifyUserCanConnectAsync(channel, user, responseChannel, cancellationToken).ConfigureAwait(false))
                    return false;
                if (!user.GetPermissions(channel).MoveMembers)
                {
                    await responseChannel.SendMessageAsync($"{options.FailureSymbol} You don't have *Move Members* permission in {GetVoiceChannelMention(channel)}.", cancellationToken).ConfigureAwait(false);
                    return false;
                }
                return true;
            }

            // verify it's a guild message
            SocketTextChannel channel = await this.VerifyGuildChannelAsync(context, cancellationToken).ConfigureAwait(false);
            if (channel == null)
                return;

            // verify command has proper arguments
            if (match.Groups.Count < 3)
            {
                string prefix = _commandsOptions.CurrentValue.Prefix;
                await context.ReplyAsync($"{options.FailureSymbol} Please specify __both__ channels IDs.\n***{_commandsOptions.CurrentValue.Prefix}move all from <original channel ID> to <target channel ID>***", cancellationToken).ConfigureAwait(false);
                return;
            }

            // verify channels exist
            // we can do both at once, it's okay if user gets warn about both at once, and it just simplifies the code
            SocketVoiceChannel channelFrom = await VerifyValidVoiceChannelAsync(match.Groups[1], context.Guild, context.Channel, cancellationToken).ConfigureAwait(false);
            SocketVoiceChannel channelTo = await VerifyValidVoiceChannelAsync(match.Groups[2], context.Guild, context.Channel, cancellationToken).ConfigureAwait(false);
            if (channelFrom == null || channelTo == null)
                return;

            // verify user can see both channels, and has move permission in both
            SocketGuildUser user = await context.Guild.GetGuildUserAsync(context.User).ConfigureAwait(false);
            _log.LogTrace("Verifying user {ID} has permission to move users between channels {ChannelFromName} ({ChannelFromID}) and {ChannelToName} ({ChannelToID})", user.Id, channelFrom.Name, channelFrom.Id, channelTo.Name, channelTo.Id);
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
            builder.AppendFormat("{0} user{3} moved from {1} to {2}.", successCount.ToString(), channelFromMention, channelToMention, successCount > 1 || successCount == 0 ? "s" : null);
            if (errCount > 0)
                builder.AppendFormat("\nFailed to move {0} user{2}. {1}", errCount.ToString(), options.FailureSymbol, errCount > 1 ? "s" : null);
            await response.ModifyAsync(props => props.Content = builder.ToString(), cancellationToken).ConfigureAwait(false);
        }

        // helper method for verifying a valid channel ID
        private async Task<SocketVoiceChannel> VerifyValidVoiceChannelAsync(Group matchGroup, IGuild guild, ISocketMessageChannel responseChannel, CancellationToken cancellationToken)
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

        private async Task<bool> VerifyUserCanConnectAsync(IVoiceChannel channel, IGuildUser user, ISocketMessageChannel responseChannel, CancellationToken cancellationToken)
        {
            if (!CanUserConnect(channel, user))
            {
                await responseChannel.SendMessageAsync($"{this._einherjiOptions.CurrentValue.FailureSymbol} You don't have access to {GetVoiceChannelMention(channel)}.", cancellationToken).ConfigureAwait(false);
                return false;
            }
            return true;
        }

        private async Task<SocketTextChannel> VerifyGuildChannelAsync(SocketCommandContext context, CancellationToken cancellationToken)
        {
            if (!(context.Channel is SocketTextChannel channel))
            {
                await context.ReplyAsync($"{this._einherjiOptions.CurrentValue.FailureSymbol} Sir, this command is only applicable in guild channels.", cancellationToken).ConfigureAwait(false);
                return null;
            }
            return channel;
        }

        private static string GetVoiceChannelMention(IVoiceChannel channel, bool isEmbed = false)
            => isEmbed ? $"[#{channel.Name}](https://discordapp.com/channels/{channel.Guild.Id}/{channel.Id})" : $"**#{channel.Name}**";

        private static bool CanUserConnect(IVoiceChannel channel, IGuildUser user)
            => user.GetPermissions(channel).Has(ChannelPermission.ViewChannel | ChannelPermission.Connect);

        private async Task<VoiceChannelMatch> MatchChannelAsync(SocketCommandContext context, Match match, int groupIndex, CancellationToken cancellationToken)
        {
            SocketVoiceChannel targetChannel = null;
            SocketGuildUser user = context.Guild.GetUser(context.User.Id);
            if (match.Groups.Count >= groupIndex + 1 && match.Groups[groupIndex].Success)
                targetChannel = await this.VerifyValidVoiceChannelAsync(match.Groups[groupIndex], context.Guild, context.Channel, cancellationToken).ConfigureAwait(false);
            else
                targetChannel = user.VoiceChannel;
            if (targetChannel == null)
            {
                await context.ReplyAsync($"{this._einherjiOptions.CurrentValue.FailureSymbol} You need to either provide a voice channel ID, or be in a voice channel currently. Also ensure I have access to that voice channel.", cancellationToken).ConfigureAwait(false);
            }
            return new VoiceChannelMatch(targetChannel, user);
        }

        private class VoiceChannelMatch
        {
            public SocketVoiceChannel Channel { get; }
            public bool IsSuccess => this.Channel != null;
            public SocketGuildUser User { get; }

            public VoiceChannelMatch(SocketVoiceChannel channel, SocketGuildUser user)
            {
                this.Channel = channel;
                this.User = user;
            }
        }

        private class VoiceChannelAction
        {
            public string ModeWord { get; init; }
            public string ActionCompletedWord { get; init; }
            public string ActionFailedWord { get; init; }
            public Action<GuildUserProperties> Action { get; }
            public ChannelPermission RequiredPermission { get; }

            public string PermissionDisplayName
            {
                get
                {
                    switch (this.RequiredPermission)
                    {
                        case ChannelPermission.DeafenMembers:
                            return "Deafen Members";
                        case ChannelPermission.MoveMembers:
                            return "Move Members";
                        case ChannelPermission.MuteMembers:
                            return "Mute Members";
                        default:
                            return this.RequiredPermission.ToString();
                    }
                }
            }

            public VoiceChannelAction(Action<GuildUserProperties> action, ChannelPermission requiredPermission, string modeWord, string completeWord, string failWord)
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
