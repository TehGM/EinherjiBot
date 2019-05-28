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
            CommandVerificator botsRedirectingVerificator = new CommandVerificator()
            {
                AcceptGuildMessages = true,
                AcceptPrivateMessages = false,
                AcceptMentionPrefix = false,
                StringPrefix = null,
                IgnoreBots = true
            };
            CommandsStack.Add(new RegexUserCommand(botsRedirectingVerificator, "^!aki", RedirectAkinatorAsync));
            CommandsStack.Add(new RegexUserCommand(botsRedirectingVerificator, "^\\?(radio|help|play|p|np|queue|skip|search|join|resume|replay|pause)", RedirectMusicBotsAsync));
            CommandsStack.Add(new RegexUserCommand(botsRedirectingVerificator, $"^<@!?{config.BotChannels.AlbionMarketID}>", RedirectAlbionMarketAsync));
        }

        private Task RedirectAlbionMarketAsync(SocketCommandContext message, Match match)
            => RedirectToChannelsAsync(message, Config.BotChannels.OtherBotsChannelsIDs, Config.BotChannels.AlbionMarketID);

        private Task RedirectMusicBotsAsync(SocketCommandContext message, Match match)
            => RedirectToChannelsAsync(message, Config.BotChannels.MusicChannelsIDs, Config.BotChannels.RythmID, Config.BotChannels.RadioinatorID);
        //{
        //    if (Config.BotChannels.MusicChannelsIDs.Contains(message.Channel.Id))
        //        return;

        //    SocketGuildUser user = await message.Guild.GetGuildUser(message.User);
        //    string channelsText = GetChannelsMentionsText(Config.BotChannels.MusicChannelsIDs, user);
        //    if (channelsText == null)
        //        return;

        //    await message.ReplyAsync($"{user.Mention}, please go to {channelsText} to use {MentionUtils.MentionUser(Config.BotChannels.RythmID)} and {MentionUtils.MentionUser(Config.BotChannels.RadioinatorID)}.");
        //}

        private Task RedirectAkinatorAsync(SocketCommandContext message, Match match)
            => RedirectToChannelsAsync(message, Config.BotChannels.AkinatorChannelsIDs, Config.BotChannels.AkinatorID);
        //{
        //    if (Config.BotChannels.AkinatorChannelsIDs.Contains(message.Channel.Id))
        //        return;

        //    SocketGuildUser user = await message.Guild.GetGuildUser(message.User);
        //    string channelsText = GetChannelsMentionsText(Config.BotChannels.AkinatorChannelsIDs, user);
        //    if (channelsText == null)
        //        return;

        //    await message.ReplyAsync($"{user.Mention}, please go to {channelsText} to use {MentionUtils.MentionUser(Config.BotChannels.AkinatorID)}.");
        //}

        #region BASE METHODS
        private async Task RedirectToChannelsAsync(SocketCommandContext message, IEnumerable<ulong> allowedChannelsIds, IEnumerable<ulong> botsIds)
        {
            if (allowedChannelsIds.Contains(message.Channel.Id))
                return;

            SocketGuildUser user = await message.Guild.GetGuildUser(message.User);
            string channelsText = GetChannelsMentionsText(allowedChannelsIds, user);
            if (channelsText == null)
                return;

            await message.ReplyAsync($"{user.Mention}, please go to {channelsText} to use {GetUsersMentionsText(botsIds)}.");
        }
        private Task RedirectToChannelsAsync(SocketCommandContext message, IEnumerable<ulong> allowedChannelsIds, params ulong[] botsIds)
            => RedirectToChannelsAsync(message, allowedChannelsIds, botsIds as IEnumerable<ulong>);

        private static string GetChannelsMentionsText(IEnumerable<ulong> ids, SocketGuildUser user)
            => GetMentionsText(ids.Where(id => user.Guild.GetChannel(id) != null
                && user.GetPermissions(user.Guild.GetChannel(id)).Has(ChannelPermission.ViewChannel | ChannelPermission.SendMessages)),
                id => MentionUtils.MentionChannel(id));

        private static string GetUsersMentionsText(IEnumerable<ulong> ids)
            => GetMentionsText(ids, id => MentionUtils.MentionUser(id), " and ");

        private static string GetMentionsText(IEnumerable<ulong> ids, Func<ulong, string> processingMethod, string lastSeparator = " or ", string normalSeparator = ", ")
        {
            int count = ids?.Count() ?? default;
            if (count == default)
                return null;
            string lastMention = processingMethod(ids.Last());
            if (count == 1)
                return lastMention;
            StringBuilder builder = new StringBuilder();
            // separate all except last with commas
            builder.AppendJoin(normalSeparator, ids.Take(count - 1).Select(i => processingMethod(i)));
            // add last with "or"
            builder.Append(lastSeparator);
            builder.Append(lastMention);
            return builder.ToString();
        }
        #endregion
    }
}
