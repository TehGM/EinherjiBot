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
    [ProductionOnly]
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

        private Task RedirectAkinatorAsync(SocketCommandContext message, Match match)
            => RedirectToChannelsAsync(message, Config.BotChannels.AkinatorChannelsIDs, Config.BotChannels.AkinatorID);

        #region BASE METHODS
        private async Task RedirectToChannelsAsync(SocketCommandContext message, IEnumerable<ulong> allowedChannelsIds, IEnumerable<ulong> botsIds)
        {
            if (allowedChannelsIds.Contains(message.Channel.Id))
                return;
            if (Config.BotChannels.IgnoreChannelsIDs.Contains(message.Channel.Id))
                return;
            if (Config.BotChannels.IgnoreUsersIDs.Contains(message.User.Id))
                return;

            SocketGuildUser user = await message.Guild.GetGuildUser(message.User);
            string channelsText = GetChannelsMentionsText(allowedChannelsIds, user);
            if (channelsText == null)
                return;

            await message.ReplyAsync($"{Config.DefaultReject} {user.Mention}, please go to {channelsText} to use {GetUsersMentionsText(botsIds)}.");
        }
        private Task RedirectToChannelsAsync(SocketCommandContext message, IEnumerable<ulong> allowedChannelsIds, params ulong[] botsIds)
            => RedirectToChannelsAsync(message, allowedChannelsIds, botsIds as IEnumerable<ulong>);

        private static string GetChannelsMentionsText(IEnumerable<ulong> ids, SocketGuildUser user)
            => ids.Where(id => user.Guild.GetChannel(id) != null
                && user.GetPermissions(user.Guild.GetChannel(id)).Has(ChannelPermission.ViewChannel | ChannelPermission.SendMessages))
                .Select(id => MentionUtils.MentionChannel(id)).JoinAsSentence(", ", " or ");

        private static string GetUsersMentionsText(IEnumerable<ulong> ids)
            => ids.Select(id => MentionUtils.MentionUser(id)).JoinAsSentence(", ", " and ");
        #endregion
    }
}
