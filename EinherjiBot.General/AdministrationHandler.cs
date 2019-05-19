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
    //[ProductionOnly]
    class AdministrationHandler : HandlerBase
    {
        public AdministrationHandler(DiscordSocketClient client, BotConfig config) 
            : base(client, config)
        {
            CommandsStack.Add(new RegexUserCommand("^purge(?:\\s+(\\d+))?", CmdPurge));
            CommandsStack.Add(new RegexUserCommand("^kick(?: \\\\?<@!?(\\d+)>)?(?: (.+))?", CmdKick));
            CommandsStack.Add(new RegexUserCommand("^ban(?: \\\\?<@!?(\\d+)>)?(?: (.+))?", CmdBan));
            CommandsStack.Add(new RegexUserCommand("^unban(?: \\\\?<@!?(\\d+)>)?", CmdUnban));
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
            await channel.DeleteMessagesAsync(msgs);
            RestUserMessage confirmationMsg = count > 0 ?
                await channel.SendMessageAsync($"Sir, your message and {count} previous message{(count > 1 ? "s were" : " was")} taken down.") :
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
