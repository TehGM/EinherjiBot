using Discord;
using Discord.Interactions;

namespace TehGM.EinherjiBot.Administration.Commands
{
    // TODO: this class is ugly as fuck - think of a way to refactor
    // I tried my best to reduce repetition, but it's still not clean at all
    // I must be missing something...

    [Group("voice", "Allows managing users in voice channels")]
    [EnabledInDm(false)]
    public class VoiceAdministrationSlashCommands : EinherjiInteractionModule
    {
        [SlashCommand("mute-all", "Mutes all users in a voice channel")]
        [DefaultMemberPermissions(GuildPermission.MuteMembers)]
        public Task CmdMuteAllAsync(
            [Summary("Channel", "Channel to mute everyone in")] IVoiceChannel channel)
            => this.PerformChannelActionAsync(u => u.Mute = true, 
                channel, null, 
                ChannelPermission.MuteMembers,
                new VoiceChannelOperationMessages(VoiceChannelOperationWords.Mute));

        [SlashCommand("unmute-all", "Unmutes all users in a voice channel")]
        [DefaultMemberPermissions(GuildPermission.MuteMembers)]
        public Task CmdUnmuteAllAsync(
            [Summary("Channel", "Channel to unmute everyone in")] IVoiceChannel channel)
            => this.PerformChannelActionAsync(u => u.Mute = false, 
                channel, null, 
                ChannelPermission.MuteMembers,
                new VoiceChannelOperationMessages(VoiceChannelOperationWords.Unmute));

        [SlashCommand("deafen-all", "Deafens all users in a voice channel")]
        [DefaultMemberPermissions(GuildPermission.DeafenMembers)]
        public Task CmdDeafenAllAsync(
            [Summary("Channel", "Channel to mute everyone in")] IVoiceChannel channel)
            => this.PerformChannelActionAsync(u => u.Deaf = true, 
                channel, null, 
                ChannelPermission.DeafenMembers,
                new VoiceChannelOperationMessages(VoiceChannelOperationWords.Deafen));

        [SlashCommand("undeafen-all", "Undeafens all users in a voice channel")]
        [DefaultMemberPermissions(GuildPermission.DeafenMembers)]
        public Task CmdUndeafenAllAsync(
            [Summary("Channel", "Channel to unmute everyone in")] IVoiceChannel channel)
            => this.PerformChannelActionAsync(u => u.Deaf = false, 
                channel, null, 
                ChannelPermission.DeafenMembers,
                new VoiceChannelOperationMessages(VoiceChannelOperationWords.Undeafen));

        [SlashCommand("move-all", "Moves all users from one voice channel to another")]
        [DefaultMemberPermissions(GuildPermission.MoveMembers)]
        public Task CmdMoveAllAsync(
            [Summary("From", "Channel to move users from")] IVoiceChannel channelFrom,
            [Summary("To", "Channel to move users to")] IVoiceChannel channelTo)
            => this.PerformChannelActionAsync(u => u.Channel = new Optional<IVoiceChannel>(channelTo),
                channelFrom, channelTo,
                ChannelPermission.MoveMembers,
                new VoiceChannelOperationMessages(VoiceChannelOperationWords.Move)
                {
                    CallerMissingSourcePermissions = "You don't have permissions to move users from channel {0}.",
                    BotMissingSourcePermissions = "I don't have permissions to move users from channel {0}.",
                    CallerMissingTargetPermissions = "You don't have permissions to move users to channel {1}.",
                    BotMissingTargetPermissions = "I don't have permissions to move users to channel {1}.",
                    Finished = "Moved {3} users from channel {0} to {1}.",
                    Failed = "Failed to move {4} users."
                });

        [SlashCommand("disconnect-all", "Disconnects all users from a voice channel")]
        [DefaultMemberPermissions(GuildPermission.MoveMembers)]
        public Task CmdDisconnectAllAsync(
            [Summary("Channel", "Channel to disconnect users from")] IVoiceChannel channel)
            => this.PerformChannelActionAsync(u => u.Channel = null,
                channel, null,
                ChannelPermission.MoveMembers,
                new VoiceChannelOperationMessages(VoiceChannelOperationWords.Move)
                {
                    CallerMissingSourcePermissions = "You don't have permissions to disconnect users from channel {0}.",
                    BotMissingSourcePermissions = "I don't have permissions to disconnect users from channel {0}.",
                    Finished = "Disconnected {3} users from channel {0}.",
                });

        private async Task PerformChannelActionAsync(Action<GuildUserProperties> action, IVoiceChannel channel, IVoiceChannel targetChannel, ChannelPermission requiredPermission, VoiceChannelOperationMessages messages)
        {
            int totalCount = 0;
            int failedCount = 0;

            // verify src channel perms
            if (!await this.VerifyCallerPermissionAsync(channel, ChannelPermission.MuteMembers).ConfigureAwait(false))
            {
                await base.RespondAsync($"{EinherjiEmote.FailureSymbol} {FormatOperationMessage(messages.CallerMissingSourcePermissions)}",
                    ephemeral: true, options: base.GetRequestOptions()).ConfigureAwait(false);
                return;
            }
            if (!await this.VerifyBotPermissionAsync(channel, ChannelPermission.MuteMembers).ConfigureAwait(false))
            {
                await base.RespondAsync($"{EinherjiEmote.FailureSymbol} {FormatOperationMessage(messages.BotMissingSourcePermissions)}",
                    ephemeral: true, options: base.GetRequestOptions()).ConfigureAwait(false);
                return;
            }

            // verify target channel perms
            if (targetChannel != null)
            {
                if (!await this.VerifyCallerPermissionAsync(targetChannel, ChannelPermission.MuteMembers).ConfigureAwait(false))
                {
                    await base.RespondAsync($"{EinherjiEmote.FailureSymbol} {FormatOperationMessage(messages.CallerMissingTargetPermissions)}",
                        ephemeral: true, options: base.GetRequestOptions()).ConfigureAwait(false);
                    return;
                }
                if (!await this.VerifyBotPermissionAsync(targetChannel, ChannelPermission.MuteMembers).ConfigureAwait(false))
                {
                    await base.RespondAsync($"{EinherjiEmote.FailureSymbol} {FormatOperationMessage(messages.BotMissingTargetPermissions)}",
                        ephemeral: true, options: base.GetRequestOptions()).ConfigureAwait(false);
                    return;
                }
            }

            // get users
            IEnumerable<IGuildUser> users = await channel.Guild.GetUsersAsync(CacheMode.AllowDownload, base.GetRequestOptions()).ConfigureAwait(false);
            users = users.Where(u => u.VoiceChannel?.Id == channel.Id);
            totalCount = users.Count();
            if (!users.Any())
            {
                await base.RespondAsync($"{EinherjiEmote.FailureSymbol} {FormatOperationMessage(messages.NoUsersInChannel)}", base.CancellationToken).ConfigureAwait(false);
                return;
            }

            // execute action
            await base.RespondAsync($"{FormatOperationMessage(messages.InProgress)}..").ConfigureAwait(false);
            foreach (IGuildUser user in users)
            {
                try
                {
                    await user.ModifyAsync(u => action(u), base.CancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    failedCount++;
                }
            }

            string responseMessage = $"{EinherjiEmote.SuccessSymbol} {FormatOperationMessage(messages.Finished)}";
            if (failedCount != 0)
                responseMessage += $"{EinherjiEmote.FailureSymbol} {FormatOperationMessage(messages.Failed)}";
            await base.ModifyOriginalResponseAsync(msg => msg.Content = responseMessage, base.GetRequestOptions()).ConfigureAwait(false);

            string FormatOperationMessage(string format)
            {
                return string.Format(format,
                    GetVoiceChannelMention(channel),
                    targetChannel != null ? GetVoiceChannelMention(targetChannel) : null,
                    totalCount, totalCount - failedCount, failedCount);
            }
        }

        private static string GetVoiceChannelMention(IVoiceChannel channel, bool isEmbed = false)
            => channel.Mention;
        //private static string GetVoiceChannelMention(IVoiceChannel channel, bool isEmbed = false)
        //    => isEmbed ? $"[#{channel.Name}](https://discordapp.com/channels/{channel.Guild.Id}/{channel.Id})" : $"**#{channel.Name}**";
        private Task<bool> VerifyBotPermissionAsync(IVoiceChannel channel, ChannelPermission permission)
            => this.VerifyPermissionAsync(base.Context.Client.CurrentUser.Id, channel, permission);
        private Task<bool> VerifyCallerPermissionAsync(IVoiceChannel channel, ChannelPermission permission)
            => this.VerifyPermissionAsync(base.Context.User.Id, channel, permission);
        private async Task<bool> VerifyPermissionAsync(ulong userID, IVoiceChannel channel, ChannelPermission permission)
        {
            IGuildUser user = await base.Context.Guild.GetGuildUserAsync(userID, base.CancellationToken).ConfigureAwait(false);

            if (user.GuildPermissions.Administrator)
                return true;

            ChannelPermissions perms = user.GetPermissions(channel);
            return perms.Has(permission) && perms.Has(ChannelPermission.ViewChannel);
        }

        private class VoiceChannelOperationMessages
        {
            public VoiceChannelOperationWords Words { get; }

            public string CallerMissingSourcePermissions { get; set; }
            public string BotMissingSourcePermissions { get; set; }
            public string CallerMissingTargetPermissions { get; set; }
            public string BotMissingTargetPermissions { get; set; }
            public string NoUsersInChannel { get; set; }
            public string InProgress { get; set; }
            public string Finished { get; set; }
            public string Failed { get; set; }

            // {0} - src channel
            // {1} - target channel
            // {2} - total count
            // {3} - success count
            // {4} - failed count
            public VoiceChannelOperationMessages(VoiceChannelOperationWords words)
            {
                this.Words = words;

                string op = words.OperationType.ToLower();
                this.CallerMissingSourcePermissions = $"You don't have permissions to {op} users in channel {{0}}.";
                this.BotMissingSourcePermissions = $"I don't have permissions to {op} users in channel {{0}}.";
                this.CallerMissingTargetPermissions = $"You don't have permissions to {op} users in channel {{1}}.";
                this.BotMissingTargetPermissions = $"I don't have permissions to {op} users in channel {{1}}.";
                this.NoUsersInChannel = "No users currently connected to channel {0}.";
                this.InProgress = $"{words.OperationInProgress} {{2}} users in channel {{0}}.";
                this.Finished = $"{words.OperationFinished} {{3}} users in channel {{0}}.";
                this.Failed = $"Failed to {op} {{4}} users.";
            }

            public VoiceChannelOperationMessages()
                : this(VoiceChannelOperationWords.Modify) { }
        }

        private class VoiceChannelOperationWords
        {
            public static VoiceChannelOperationWords Modify { get; } = new VoiceChannelOperationWords("Modify", "Modifying", "Modified");
            public static VoiceChannelOperationWords Mute { get; } = new VoiceChannelOperationWords("Mute", "Muting", "Muted");
            public static VoiceChannelOperationWords Unmute { get; } = new VoiceChannelOperationWords("Unmute", "Unmuting", "Unmuted");
            public static VoiceChannelOperationWords Deafen { get; } = new VoiceChannelOperationWords("Deafen", "Deafening", "Deafened");
            public static VoiceChannelOperationWords Undeafen { get; } = new VoiceChannelOperationWords("Undeafen", "Undeafening", "Undeafened");
            public static VoiceChannelOperationWords Move { get; } = new VoiceChannelOperationWords("Move", "Moving", "Moved");
            public static VoiceChannelOperationWords Disconnect { get; } = new VoiceChannelOperationWords("Disconnect", "Disconnecting", "Disconnected");

            public string OperationType { get; }
            public string OperationInProgress { get; }
            public string OperationFinished { get; }

            private VoiceChannelOperationWords(string operationType, string operationInProgress, string operationFinished)
            {
                this.OperationType = operationType;
                this.OperationInProgress = operationInProgress;
                this.OperationFinished = operationFinished;
            }
        }
    }
}
