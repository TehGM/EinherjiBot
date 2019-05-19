using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using TehGM.EinherjiBot.CommandsProcessing;
using TehGM.EinherjiBot.Config;
using TehGM.EinherjiBot.Extensions;
using Discord;

namespace TehGM.EinherjiBot
{
    class BotChannelsInfoHandler : HandlerBase
    {
        public BotChannelsInfoHandler(DiscordSocketClient client, BotConfig config) : base(client, config)
        {
            CommandVerificator botsForwardingVerificator = new CommandVerificator()
            {
                AcceptGuildMessages = true,
                AcceptPrivateMessages = false,
                AcceptMentionPrefix = false,
                StringPrefix = null,
                IgnoreBots = true
            };
            CommandsStack.Add(new RegexUserCommand(botsForwardingVerificator, "^!aki", ForwardAkinatorAsync));
            CommandsStack.Add(new RegexUserCommand(botsForwardingVerificator, "^\\?(radio|help|play|p|np|queue|skip|search|join|resume|replay|pause)", ForwardMusicBotsAsync));
        }

        private async Task ForwardMusicBotsAsync(SocketCommandContext message, Match match)
        {
            if (message.IsPrivate)
                return;
            if (Config.BotChannels.MusicChannelsIDs.Contains(message.Channel.Id))
                return;

            SocketGuildUser user = await message.Guild.GetGuildUser(message.User);
            string channelsText = GetChannelsMentionsText(Config.BotChannels.MusicChannelsIDs, user);
            if (channelsText == null)
                return;

            await message.ReplyAsync($"{user.Mention}, please go to {channelsText} to use {MentionUtils.MentionUser(Config.BotChannels.RythmID)} and {MentionUtils.MentionUser(Config.BotChannels.RadioinatorID)}.");
        }

        private async Task ForwardAkinatorAsync(SocketCommandContext message, Match match)
        {
            if (message.IsPrivate)
                return;
            if (Config.BotChannels.AkinatorChannelsIDs.Contains(message.Channel.Id))
                return;

            SocketGuildUser user = await message.Guild.GetGuildUser(message.User);
            string channelsText = GetChannelsMentionsText(Config.BotChannels.AkinatorChannelsIDs, user);
            if (channelsText == null)
                return;

            await message.ReplyAsync($"{user.Mention}, please go to {channelsText} to use {MentionUtils.MentionUser(Config.BotChannels.AkinatorID)}.");
        }

        private static string GetChannelsMentionsText(IEnumerable<ulong> ids, SocketGuildUser user)
            => GetMentionsText(ids.Where(id => user.Guild.GetChannel(id) != null 
                && user.GetPermissions(user.Guild.GetChannel(id)).Has(ChannelPermission.ViewChannel | ChannelPermission.SendMessages)),
                id => MentionUtils.MentionChannel(id));

        private static string GetMentionsText(IEnumerable<ulong> ids, Func<ulong, string> processingMethod)
        {
            int count = ids?.Count() ?? default;
            if (count == default)
                return null;
            string lastRoleMention = processingMethod(ids.Last());
            if (count == 1)
                return lastRoleMention;
            StringBuilder builder = new StringBuilder();
            // separate all except last with commas
            builder.AppendJoin(", ", ids.Take(count - 1).Select(i => processingMethod(i)));
            // add last with "or"
            builder.Append(" or ");
            builder.Append(lastRoleMention);
            return builder.ToString();
        }
    }
}
