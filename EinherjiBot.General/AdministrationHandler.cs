using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using TehGM.EinherjiBot.Config;
using TehGM.EinherjiBot.Extensions;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    [ProductionOnly]
    class AdministrationHandler : HandlerBase
    {
        public AdministrationHandler(DiscordSocketClient client, BotConfig config) 
            : base(client, config)
        {
            CommandsStack.Add(new RegexUserCommand("^purge(?:\\s+(\\d+))?", CmdPurge));
            //CommandsStack.Add(new RegexUserCommand("^kick(?: \\\\?<@!?(\\d+)>)?(?: (.+))?", CmdKick));
            //CommandsStack.Add(new RegexUserCommand("^ban(?: \\\\?<@!?(\\d+)>)?(?: (.+))?", CmdBan));
            //CommandsStack.Add(new RegexUserCommand("^unban(?: \\\\?<@!?(\\d+)>)?", CmdUnban));
            CommandsStack.Add(new RegexUserCommand("^move\\s?all(?:(?: from)?\\s+(?:<#)?(\\d+)(?:>)?)?(?:(?: to)?\\s+(?:<#)?(\\d+)(?:>)?)?", CmdMoveAll));
        }

        private async Task CmdMoveAll(SocketCommandContext message, Match match)
        {
            async Task<SocketVoiceChannel> VerifyValidChannelAsync(Group matchGroup, IGuild guild, ISocketMessageChannel responseChannel)
            {
                if (matchGroup == null || !match.Success || match.Length < 1)
                    return null;
                if (!ulong.TryParse(matchGroup.Value, out ulong id))
                {
                    await responseChannel.SendMessageAsync($"{Config.DefaultReject} `{matchGroup.Value}` is not a valid channel ID.`");
                    return null;
                }

                // instead of doing quick check, do a series to help user pinpoint any issue
                // find channel first
                SocketChannel channel = Client.GetChannel(id);
                if (channel == null || !(channel is SocketGuildChannel guildChannel))
                {
                    await responseChannel.SendMessageAsync($"{Config.DefaultReject} I don't know any guild channel with ID `{id.ToString()}`.");
                    return null;
                }

                // verify channel is in guild
                if (guildChannel.Guild.Id != guild.Id)
                {
                    await responseChannel.SendMessageAsync($"{Config.DefaultReject} Channel **#{guildChannel.Name}** doesn't exist in **{guild.Name}** guild.");
                    return null;
                }

                // lastly make sure it is a voice channel
                if (!(guildChannel is SocketVoiceChannel voiceChannel))
                {
                    await responseChannel.SendMessageAsync($"{Config.DefaultReject} {MentionUtils.MentionChannel(id)} is not a voice channel.");
                    return null;
                }

                return voiceChannel;
            }
            bool CanUserConnect(IVoiceChannel channel, IGuildUser user)
                => user.GetPermissions(channel).Has(ChannelPermission.ViewChannel | ChannelPermission.Connect);
            string GetVoiceChannelMention(IVoiceChannel channel, bool isEmbed = false)
                => isEmbed ? $"[#{channel.Name}](https://discordapp.com/channels/{channel.Guild.Id}/{channel.Id})" : $"**#{channel.Name}**";
            async Task<bool> VerifyUserCanMoveAsync(IVoiceChannel channel, IGuildUser user, ISocketMessageChannel responseChannel)
            {
                if (!CanUserConnect(channel, user))
                {
                    await responseChannel.SendMessageAsync($"{Config.DefaultReject} You don't have access to {GetVoiceChannelMention(channel)}.");
                    return false;
                }
                if (!user.GetPermissions(channel).MoveMembers)
                {

                    await responseChannel.SendMessageAsync($"{Config.DefaultReject} You don't have *Move Members* permission in {GetVoiceChannelMention(channel)}.");
                    return false;
                }
                return true;
            }

            // verify it's a guild message
            if (!(message.Channel is SocketTextChannel channel))
            {
                await message.ReplyAsync($"{Config.DefaultReject} Sir, this command is only applicable in guild channels.");
                return;
            }

            // verify command has proper arguments
            if (match.Groups.Count < 3)
            {
                await message.ReplyAsync($"{Config.DefaultReject} Please specify __both__ channels IDs.\n***{GetDefaultPrefix()}move all from <original channel ID> to <target channel ID>***");
                return;
            }

            // verify channels exist
            // we can do both at once, it's okay if user gets warn about both at once, and it just simplifies the code
            SocketVoiceChannel channelFrom = await VerifyValidChannelAsync(match.Groups[1], message.Guild, message.Channel);
            SocketVoiceChannel channelTo = await VerifyValidChannelAsync(match.Groups[2], message.Guild, message.Channel);
            if (channelFrom == null || channelTo == null)
                return;

            // verify user can see both channels, and has move permission in both
            SocketGuildUser user = await message.Guild.GetGuildUser(message.User);
            if (!user.GuildPermissions.Administrator)
            {
                if (!await VerifyUserCanMoveAsync(channelFrom, user, channel)
                    || !await VerifyUserCanMoveAsync(channelTo, user, channel))
                    return;
            }

            // move the users
            SocketGuildUser[] users = channelFrom.Users.ToArray();
            string channelFromMention = GetVoiceChannelMention(channelFrom);
            string channelToMention = GetVoiceChannelMention(channelTo);
            RestUserMessage response = await message.ReplyAsync($"Moving {users.Length.ToString()} users from {channelFromMention} to {channelToMention}.");
            int errCount = 0;
            for (int i = 0; i < users.Length; i++)
            {
                try
                {
                    await users[i].ModifyAsync(props => props.Channel = channelTo);
                }
                catch { errCount++; }
            }
            // display confirmation
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0} users moved from {1} to {2}.", (users.Length - errCount).ToString(), channelFromMention, channelToMention);
            if (errCount > 0)
                builder.AppendFormat("\nFailed to move {0} users. {1}", errCount.ToString(), Config.DefaultReject);
            await response.ModifyAsync(props => props.Content = builder.ToString());
        }

        private async Task CmdPurge(SocketCommandContext message, Match match)
        {
            if (!(message.Channel is SocketTextChannel channel))
            {
                await message.ReplyAsync("Sir, this command is only applicable in guild channels.");
                return;
            }
            SocketGuildUser user = await message.Guild.GetGuildUser(message.User);
            if (!user.GetPermissions(channel).ManageMessages)
            {
                await channel.SendMessageAsync("You can't order me to do that.");
                return;
            }
            if (match.Groups.Count == 1 || match.Groups[1]?.Length < 1)
            {
                await channel.SendMessageAsync("Sir, I need a positive number of messages to take down.");
                return;
            }
            string countString = match.Groups[1].Value;
            if (!int.TryParse(countString, out int count))
            {
                await channel.SendMessageAsync($"Sir, `{countString} is not a valid number.");
                return;
            }
            if (count < 0)
            {
                await channel.SendMessageAsync($"Sir, how am I supposed to execute removal of {count} messages?.");
                return;
            }

            // get last X messages
            var msgs = await channel.GetMessagesAsync(count + 1).FlattenAsync();
            int actualCount = msgs.Count() - 1;
            await channel.DeleteMessagesAsync(msgs);
            RestUserMessage confirmationMsg = actualCount > 0 ?
                await channel.SendMessageAsync($"Sir, your message and {actualCount} previous message{(actualCount > 1 ? "s were" : " was")} taken down.") :
                await channel.SendMessageAsync($"Sir, I deleted your message. Specify count greater than 0 to remove more than just that.");
            await Task.Delay(6 * 1000);
            await channel.DeleteMessageAsync(confirmationMsg);
        }

        const string ErrorGuildOnlyText = "Sir, this command is only applicable in guild channels.";
        const string ErrorUserNoPermsText = "You can't order me to do that.";
        const string ErrorBotNoPermsText = "I have no permissions to do that.";
        const string ErrorMissingUserPingText = "Sir, please broadcast target member in your command.";

        private async Task CmdKick(SocketCommandContext message, Match match)
        {
            if (await EnsureGuildChannelAsync(message) == null)
                return;
            if (await EnsureUserHasPermissionAsync(message, GuildPermission.KickMembers) == null)
                return;
            if (await EnsureBotHasPermissionAsync(message, GuildPermission.KickMembers) == null)
                return;

            IUser targetUser = await ParseUserFromArgument(message, match, 1, userId => $"User with ID {userId} not recognized.");
            if (targetUser == null)
                return;
            SocketGuildUser targetGuildUser = await message.Guild.GetGuildUser(targetUser);
            if (targetGuildUser == null)
            {
                await message.ReplyAsync($"{targetUser.Mention} does not appear to be in **{message.Guild.Name}**.");
                return;
            }
            string reason = null;
            if (match.Groups.Count > 2 && match.Groups[2]?.Value?.Trim().Length > 0)
                reason = match.Groups[2].Value;
            await targetGuildUser.KickAsync(reason);
            await message.ReplyAsync($"Kicked {targetUser.Mention}.{BuildReasonAppendText(reason)}");
        }

        private async Task CmdBan(SocketCommandContext message, Match match)
        {
            if (await EnsureGuildChannelAsync(message) == null)
                return;
            if (await EnsureUserHasPermissionAsync(message, GuildPermission.BanMembers) == null)
                return;
            if (await EnsureBotHasPermissionAsync(message, GuildPermission.BanMembers) == null)
                return;

            IUser targetUser = await ParseUserFromArgument(message, match, 1, userId => $"User with ID {userId} not recognized.");
            if (targetUser == null)
                return;
            IReadOnlyCollection<RestBan> bans = await message.Guild.GetBansAsync();
            RestBan existingBan = bans.FirstOrDefault(b => b.User.Id == targetUser.Id);
            if (existingBan != null)
            {
                await message.ReplyAsync($"{targetUser.Mention} is already banned.{BuildReasonAppendText(existingBan.Reason)}");
                return;
            }
            string reason = null;
            if (match.Groups.Count > 2 && match.Groups[2]?.Value?.Trim().Length > 0)
                reason = match.Groups[2].Value;
            await message.Guild.AddBanAsync(targetUser, 0, reason);
            await message.ReplyAsync($"Banned {targetUser.Mention}.{BuildReasonAppendText(reason)}");
        }
        private async Task CmdUnban(SocketCommandContext message, Match match)
        {
            if (await EnsureGuildChannelAsync(message) == null)
                return;
            if (await EnsureUserHasPermissionAsync(message, GuildPermission.BanMembers) == null)
                return;
            if (await EnsureBotHasPermissionAsync(message, GuildPermission.BanMembers) == null)
                return;

            IUser targetUser = await ParseUserFromArgument(message, match, 1, userId => $"User with ID {userId} not recognized.");
            if (targetUser == null)
                return;
            IReadOnlyCollection<RestBan> bans = await message.Guild.GetBansAsync();
            RestBan existingBan = bans.FirstOrDefault(b => b.User.Id == targetUser.Id);
            if (existingBan == null)
            {
                await message.ReplyAsync($"{targetUser.Mention} is not banned.");
                return;
            }
            await message.Guild.RemoveBanAsync(targetUser);
            await message.ReplyAsync($"Unbanned {targetUser.Mention}.");
        }

        private static string BuildReasonAppendText(string reason)
            => string.IsNullOrWhiteSpace(reason) ? null : $" Reason `{reason}`.";

        // returns null if not guild channel
        private async Task<SocketTextChannel> EnsureGuildChannelAsync(SocketCommandContext command, 
            string errorResponse = ErrorGuildOnlyText)
        {
            if (!(command.Channel is SocketTextChannel channel))
            {
                await command.ReplyAsync(errorResponse);
                return null;
            }
            return channel;
        }

        // returns null if no permission
        private async Task<SocketGuildUser> EnsureUserHasPermissionAsync(SocketCommandContext command,
            ChannelPermission permission, string errorResponse = ErrorUserNoPermsText)
            => await EnsureUserHasPermissionInternalAsync(command, await command.Guild.GetGuildUser(command.User), user => user.GetPermissions(command.Channel as IGuildChannel).Has(permission), errorResponse);

        // returns null if no permission
        private async Task<SocketGuildUser> EnsureUserHasPermissionAsync(SocketCommandContext command,
            GuildPermission permission, string errorResponse = ErrorUserNoPermsText)
            => await EnsureUserHasPermissionInternalAsync(command, await command.Guild.GetGuildUser(command.User), user => user.GuildPermissions.Has(permission), errorResponse);

        // returns null if no permission
        private async Task<SocketGuildUser> EnsureUserHasPermissionInternalAsync(SocketCommandContext command, SocketGuildUser user,
            Func<SocketGuildUser, bool> CheckMethod, string errorResponse = ErrorUserNoPermsText)
        {
            if (user == null)
                return null;
            if (!CheckMethod(user))
            {
                await command.ReplyAsync(errorResponse);
                return null;
            }
            return user;
        }

        // returns null if no permission
        private Task<SocketGuildUser> EnsureBotHasPermissionAsync(SocketCommandContext command,
            ChannelPermission permission, string errorResponse = ErrorBotNoPermsText)
            => EnsureUserHasPermissionInternalAsync(command, command.Guild.CurrentUser, user => user.GetPermissions(command.Channel as IGuildChannel).Has(permission), errorResponse);

        // returns null if no permission
        private Task<SocketGuildUser> EnsureBotHasPermissionAsync(SocketCommandContext command,
            GuildPermission permission, string errorResponse = ErrorBotNoPermsText)
            => EnsureUserHasPermissionInternalAsync(command, command.Guild.CurrentUser, user => user.GuildPermissions.Has(permission), errorResponse);

        // return null if any error
        private async Task<IUser> ParseUserFromArgument(SocketCommandContext command, Match match, int argGroupIndex,
            Func<string, string> userNotFoundErrorResponse, string missingPingErrorResponse = ErrorMissingUserPingText)
        {
            if (match.Groups.Count <= argGroupIndex || match.Groups[argGroupIndex]?.Length < 1)
            {
                await command.ReplyAsync(missingPingErrorResponse);
                return null;
            }
            string idString = match.Groups[argGroupIndex].Value;
            if (!ulong.TryParse(idString, out ulong id))
            {
                await command.ReplyAsync(userNotFoundErrorResponse(idString));
                return null;
            }
            IUser user = await Client.GetUserAsync(id);
            if (user == null)
            {
                await command.ReplyAsync(userNotFoundErrorResponse(idString));
                return null;
            }
            return user;
        }
    }
}
