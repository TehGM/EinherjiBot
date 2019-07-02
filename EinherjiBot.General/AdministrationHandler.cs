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
            RestUserMessage response = await message.ReplyAsync($"Moving {users.Length.ToString()} user{(users.Length > 1 ? "s" : null)} from {channelFromMention} to {channelToMention}.");
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
            int successCount = users.Length - errCount;
            builder.AppendFormat("{0} user{3} moved from {1} to {2}.", successCount.ToString(), channelFromMention, channelToMention, successCount > 1 ? "s" : null);
            if (errCount > 0)
                builder.AppendFormat("\nFailed to move {0} user{2}. {1}", errCount.ToString(), Config.DefaultReject, errCount > 1 ? "s" : null);
            await response.ModifyAsync(props => props.Content = builder.ToString());
        }

        private async Task CmdPurge(SocketCommandContext message, Match match)
        {
            if (!(message.Channel is SocketTextChannel channel))
            {
                await message.ReplyAsync($"{Config.DefaultReject} Sir, this command is only applicable in guild channels.");
                return;
            }
            SocketGuildUser user = await message.Guild.GetGuildUser(message.User);
            if (!user.GetPermissions(channel).ManageMessages)
            {
                await channel.SendMessageAsync($"{Config.DefaultReject} You can't order me to do that.");
                return;
            }
            if (match.Groups.Count == 1 || match.Groups[1]?.Length < 1)
            {
                await channel.SendMessageAsync($"{Config.DefaultReject} Sir, I need a positive number of messages to take down.");
                return;
            }
            string countString = match.Groups[1].Value;
            if (!int.TryParse(countString, out int count))
            {
                await channel.SendMessageAsync($"{Config.DefaultReject} Sir, `{countString} is not a valid number.");
                return;
            }
            if (count < 0)
            {
                await channel.SendMessageAsync($"{Config.DefaultReject} Sir, how am I supposed to execute removal of {count} messages?.");
                return;
            }

            // get last X messages
            var msgs = await channel.GetMessagesAsync(count + 1).FlattenAsync();
            int actualCount = msgs.Count() - 1;
            await channel.DeleteMessagesAsync(msgs);
            RestUserMessage confirmationMsg = actualCount > 0 ?
                await channel.SendMessageAsync($"{Config.DefaultConfirm} Sir, your message and {actualCount} previous message{(actualCount > 1 ? "s were" : " was")} taken down.") :
                await channel.SendMessageAsync($"{Config.DefaultConfirm} Sir, I deleted your message. Specify count greater than 0 to remove more than just that.");
            await Task.Delay(6 * 1000);
            await channel.DeleteMessageAsync(confirmationMsg);
        }
    }
}
