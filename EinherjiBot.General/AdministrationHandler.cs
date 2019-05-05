using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using TehGM.EinherjiBot.Config;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    class AdministrationHandler : HandlerBase
    {
        public AdministrationHandler(DiscordSocketClient client, BotConfig config) 
            : base(client, config)
        {
            CommandsStack.Add(new RegexUserCommand("^\\s*purge(?:\\s+(\\d+))?", CmdPurge));
        }

        private async Task CmdPurge(SocketCommandContext message, Match match)
        {
            if (!(message.Channel is SocketTextChannel channel))
            {
                await message.Channel.SendMessageAsync("Sir, this command is only applicable in guild channels.");
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
            if (count < 1)
            {
                await channel.SendMessageAsync($"Sir, how am I supposed to execute removal of {count} messages?.");
                return;
            }

            // get last X messages
            var msgs = await channel.GetMessagesAsync(count).FlattenAsync();
            await channel.DeleteMessagesAsync(msgs);
            RestUserMessage confirmationMsg = await channel.SendMessageAsync($"Sir, {count} message{(count > 1 ? "s were" : " was")} taken down.");
            await Task.Delay(5 * 1000);
            await channel.DeleteMessageAsync(confirmationMsg);
        }
    }
}
