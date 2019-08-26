using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TehGM.EinherjiBot.CommandsProcessing;
using TehGM.EinherjiBot.Config;
using TehGM.EinherjiBot.DataModels.Permits;
using TehGM.EinherjiBot.Extensions;

namespace TehGM.EinherjiBot
{
    [ProductionOnly]
    class PermitsHandler : HandlerBase
    {
        public PermitsHandler(DiscordSocketClient client, BotConfig config) : base(client, config)
        {
            // netflix permit
            CommandsStack.Add(new RegexUserCommand("^netflix (?:password|account|login)", (msg, match) => CmdRetrieve(msg, match, config.Data.NetflixAccount)));
            CommandsStack.Add(new RegexUserCommand("^netflix set (login|email|username|password|pass|pwd) (.+)", (msg, match) => CmdSet(msg, match, config.Data.NetflixAccount)));
        }

        private async Task CmdRetrieve(SocketCommandContext message, Match match, PermitInfo permit)
        {
            if (!await ValidateRequestAsync(message, permit, false))
                return;

            // create message
            IUser modifiedUser = Client.GetUser(permit.LastModifiedByID);
            EmbedBuilder embed = permit.CreateConfirmationEmbed(modifiedUser);
            // send message
            RestUserMessage sentMsg = await message.ReplyAsync(GetWarningIfAutoremoving(permit, false), false, embed.Build());
            // auto remove
            if (permit.IsAutoRemoving)
                await RemoveMessagesDelayed(permit.AutoRemoveDelay, sentMsg);
        }

        private async Task CmdSet(SocketCommandContext message, Match match, PermitInfo permit)
        {
            if (!await ValidateRequestAsync(message, permit, true))
                return;

            PermitInfo.UpdateResult result = permit.Update(message, match);
            if (!result.IsSuccess)
            {
                await SendError($"{result.Message}", message.Channel);
                return;
            }
            await Config.Data.SaveAsync();
            // create message
            EmbedBuilder embed = permit
                .CreateConfirmationEmbed(message.User)
                .WithDescription(result.Message);
            //send message
            RestUserMessage sentMsg = await message.ReplyAsync(GetWarningIfAutoremoving(permit), false, embed.Build());
            // auto remove
            if (permit.IsAutoRemoving)
                await RemoveMessagesDelayed(permit.AutoRemoveDelay, sentMsg, message.Message);
        }

        private async Task<bool> ValidateRequestAsync(SocketCommandContext message, PermitInfo permit, bool modifying)
        {
            if (permit == null)
            {
                await SendError($"Configuration data missing.", message.Channel);
                return false;
            }
            if (message.IsPrivate)
            {
                await SendError($"You can't do this in private message.\nGo to {GetAllowedChannelsMentionsText(permit)}.", message.Channel);
                return false;
            }
            SocketGuildUser user = await message.Guild.GetGuildUser(message.User);
            if (!permit.CanRetrieve(user))
            {
                await SendError($"You need {GetAllowedRolesMentionsText(permit)} role to do this.", message.Channel);
                return false;
            }
            if (modifying && !permit.CanModify(user))
            {
                await SendError($"You have no permissions to do this.", message.Channel);
                return false;
            }
            if (!permit.IsChannelAllowed(message.Channel))
            {
                await SendError($"You can't do this here.\nGo to {GetAllowedChannelsMentionsText(permit)}.", message.Channel);
                return false;
            }
            return true;
        }

        private string GetAllowedRolesMentionsText(PermitInfo permit)
            => permit.RetrieveRolesID.Select(id => MentionUtils.MentionRole(id)).JoinAsSentence(lastSeparator: " or ");

        private string GetAllowedChannelsMentionsText(PermitInfo permit)
            => permit.AllowedChannelsIDs.Select(id => MentionUtils.MentionChannel(id)).JoinAsSentence(lastSeparator: " or ");

        private async Task RemoveMessagesDelayed(TimeSpan delay, params IMessage[] messages)
        {
            if (messages.Length == 0)
                return;
            SocketTextChannel channel = messages[0].Channel as SocketTextChannel;
            await Task.Delay(delay);
            await channel.DeleteMessagesAsync(messages);
        }

        private string GetWarningIfAutoremoving(PermitInfo permit, bool removingSendersMsg = true)
            => permit.IsAutoRemoving ? $"I will remove this{(removingSendersMsg ? " and your" : null)} message in {permit.AutoRemoveDelay.ToShortFriendlyString()}." : null;

        private Task SendError(string text, IMessageChannel channel, string mention = null)
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(255, 0, 0)
                .WithTitle("Error")
                .WithDescription(text);
            return channel.SendMessageAsync(mention, false, embed.Build());
        }
    }
}
