using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TehGM.EinherjiBot.CommandsProcessing;
using TehGM.EinherjiBot.Config;
using TehGM.EinherjiBot.DataModels;
using TehGM.EinherjiBot.Extensions;
using Discord.Rest;

namespace TehGM.EinherjiBot
{
    [ProductionOnly]
    public class NetflixAccountHandler : HandlerBase
    {
        private readonly NetflixAccountInfo NetflixAccount;

        public NetflixAccountHandler(DiscordSocketClient client, BotConfig config) : base(client, config)
        {
            this.NetflixAccount = config.Data.NetflixAccount;
            CommandsStack.Add(new RegexUserCommand("^netflix set (login|email|username|password|pass|pwd) (.+)", CmdSet));
            CommandsStack.Add(new RegexUserCommand("^netflix (?:password|account|login)", CmdRetrieve));
        }

        private async Task CmdRetrieve(SocketCommandContext message, Match match)
        {
            if (message.IsPrivate)
            {
                await SendError($"You can't do this in private message.\nGo to {GetAllowedChannelsMentionsText()}.", message.Channel);
                return;
            }
            SocketGuildUser user = await message.Guild.GetGuildUser(message.User);
            if (!NetflixAccount.CanRetrieve(user))
            {
                await SendError($"You need {GetAllowedRolesMentionsText()} role to do this.", message.Channel);
                return;
            }
            if (!NetflixAccount.IsChannelAllowed(message.Channel))
            {
                await SendError($"You can't do this here.\nGo to {GetAllowedChannelsMentionsText()}.", message.Channel);
                return;
            }

            // create message
            IUser modifiedUser = Client.GetUser(NetflixAccount.LastModifiedByID);
            EmbedBuilder embed = CreateConfirmationEmbed(modifiedUser);
            // send message
            RestUserMessage sentMsg = await message.ReplyAsync(GetWarningIfAutoremoving(false), false, embed.Build());
            // auto remove
            if (NetflixAccount.IsAutoRemoving)
                await RemoveMessagesDelayed(NetflixAccount.AutoRemoveDelay, sentMsg);
        }

        private async Task CmdSet(SocketCommandContext message, Match match)
        {
            if (message.IsPrivate)
            {
                await SendError($"You can't do this in private message.\nGo to {GetAllowedChannelsMentionsText()}.", message.Channel);
                return;
            }
            SocketGuildUser user = await message.Guild.GetGuildUser(message.User);
            if (!NetflixAccount.CanModify(user))
            {
                await SendError($"You have no permissions to do this.", message.Channel);
                return;
            }
            if (!NetflixAccount.IsChannelAllowed(message.Channel))
            {
                await SendError($"You can't do this here.\nGo to {GetAllowedChannelsMentionsText()}.", message.Channel);
                return;
            }

            SetMode mode = StringToSetMode(match.Groups[1].Value);
            string value = match.Groups[2].Value;
            string responseText = null;
            if (mode == SetMode.Login)
            {
                NetflixAccount.SetLogin(value, message.User.Id);
                responseText = $"You have set Netflix account login to `{value}`.";
            }
            if (mode == SetMode.Password)
            {
                NetflixAccount.SetPassword(value, message.User.Id);
                responseText = $"You have set Netflix account password to `{value}`.";
            }
            await Config.SaveAllAsync();
            // create message
            EmbedBuilder embed = CreateConfirmationEmbed(message.User);
            embed.WithDescription(responseText);
            //send message
            RestUserMessage sentMsg = await message.ReplyAsync(GetWarningIfAutoremoving(), false, embed.Build());
            // auto remove
            if (NetflixAccount.IsAutoRemoving)
                await RemoveMessagesDelayed(NetflixAccount.AutoRemoveDelay, sentMsg, message.Message);
        }

        private EmbedBuilder CreateConfirmationEmbed(IUser lastModifiedUser)
        {
            EmbedBuilder embed = new EmbedBuilder()
                .AddField("Login", NetflixAccount.Login)
                .AddField("Password", NetflixAccount.Password)
                .WithThumbnailUrl("https://historia.org.pl/wp-content/uploads/2018/04/netflix-logo.jpg");
            if (NetflixAccount.LastModifiedByID != 0)
            {
                embed.WithTimestamp(NetflixAccount.LastModifiedTimeUtc)
                .WithFooter($"Last modified by {lastModifiedUser.Username}#{lastModifiedUser.Discriminator}", lastModifiedUser.GetAvatarUrl());
            }
            SetSuccessColor(embed);
            return embed;
        }

        private string GetWarningIfAutoremoving(bool removingSendersMsg = true)
            => NetflixAccount.IsAutoRemoving ? $"I will remove this{(removingSendersMsg ? " and your" : null)} message in {NetflixAccount.AutoRemoveDelay.ToFriendlyString()}." : null;

        private Task SendError(string text, IMessageChannel channel, string mention = null)
        {
            EmbedBuilder embed = new EmbedBuilder();
            SetErrorColor(embed);
            embed.WithTitle("Error")
                .WithDescription(text);
            return channel.SendMessageAsync(mention, false, embed.Build());
        }

        private string GetAllowedRolesMentionsText()
            => GetMentionsText(NetflixAccount.RetrieveRolesID, MentionUtils.MentionRole);

        private string GetAllowedChannelsMentionsText()
            => GetMentionsText(NetflixAccount.AllowedChannelsIDs, MentionUtils.MentionChannel);

        private static string GetMentionsText(IEnumerable<ulong> ids, Func<ulong, string> processingMethod)
        {
            int count = ids.Count();
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

        private async Task RemoveMessagesDelayed(TimeSpan delay, params IMessage[] messages)
        {
            if (messages.Length == 0)
                return;
            SocketTextChannel channel = messages[0].Channel as SocketTextChannel;
            await Task.Delay(delay);
            await channel.DeleteMessagesAsync(messages);
        }

        private static EmbedBuilder SetErrorColor(EmbedBuilder embed)
            => embed.WithColor(255, 0, 0);
        private static EmbedBuilder SetSuccessColor(EmbedBuilder embed)
            => embed.WithColor(0, 255, 0);

        private static SetMode StringToSetMode(string value)
        {
            switch (value.ToLower())
            {
                case "login":
                case "email":
                case "username":
                    return SetMode.Login;
                case "password":
                case "pass":
                case "pwd":
                    return SetMode.Password;
                default:
                    throw new ArgumentException($"Invalid {nameof(SetMode)} \"{value.ToLower()}\"");
            }
        }

        private enum SetMode
        {
            Password,
            Login
        }
    }
}
