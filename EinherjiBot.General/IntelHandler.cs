using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TehGM.EinherjiBot.CommandsProcessing;
using TehGM.EinherjiBot.Config;
using TehGM.EinherjiBot.DataModels;
using TehGM.EinherjiBot.Extensions;

namespace TehGM.EinherjiBot
{
    [ProductionOnly]
    class IntelHandler : HandlerBase
    {
        public IntelHandler(DiscordSocketClient client, BotConfig config) : base(client, config)
        {
            CommandsStack.Add(new RegexUserCommand("^intel on me", CmdIntelMe));
            CommandsStack.Add(new RegexUserCommand("^intel on \\\\?<@!?(\\d+)>", CmdIntelUser));
            CommandsStack.Add(new RegexUserCommand("^intel on guild", CmdIntelGuild));
            CommandsStack.Add(new RegexUserCommand("^intel", CmdIntelHelp));
        }

        private Task CmdIntelMe(SocketCommandContext message, Match match)
            => ProcessIntelUser(message, message.User);

        private async Task CmdIntelUser(SocketCommandContext message, Match match)
        {
            string idString = match.Groups[1].Value;
            if (!ulong.TryParse(idString, out ulong id))
            {
                await message.ReplyAsync($"Could not parse user ID `{idString}`.");
                return;
            }
            IUser user = await Client.GetUserAsync(id);
            if (user == null)
            {
                await message.ReplyAsync($"Could not find user with ID `{id}`.");
                return;
            }
            await ProcessIntelUser(message, user);
        }

        private Task CmdIntelGuild(SocketCommandContext message, Match match)
        {
            if (message.IsPrivate)
                return message.ReplyAsync("This command can only be used in a guild channel.");

            EmbedBuilder embed = new EmbedBuilder();
            AddGuildInfo(embed, message.Guild);
            return message.ReplyAsync(null, false, embed.Build());
        }

        private Task CmdIntelHelp(SocketCommandContext message, Match match)
        {
            string prefix = (CommandVerificator.DefaultPrefixed as CommandVerificator).StringPrefix;
            EmbedBuilder embed = new EmbedBuilder()
                .AddField("Intel Commands",
                    $"**{prefix}intel on me** - get intel on yourself\n" +
                    $"**{prefix}intel on** ***<user ping>*** - get intel on pinged user\n" +
                    $"**{prefix}intel on guild** - *(guild only)* get intel on current guild");
            return message.ReplyAsync(null, false, embed.Build());
        }

        private Task ProcessIntelUser(SocketCommandContext message, IUser user)
        {
            EmbedBuilder embed = new EmbedBuilder();
            AddUserInfo(embed, user);
            if (!message.IsPrivate)
            {
                SocketGuildUser guildUser = message.Guild.GetUser(user.Id);
                if (guildUser != null)
                    AddGuildUserInfo(embed, guildUser);
            }
            return message.ReplyAsync(null, false, embed.Build());
        }

        protected override Task OnGuildMemberUpdated(SocketGuildUser userBefore, SocketGuildUser userAfter)
        {
            if (userBefore.Status == userAfter.Status)
                return Task.CompletedTask;
            if (userAfter.Status != UserStatus.Offline && userBefore.Status != UserStatus.Offline)
                return Task.CompletedTask;
            UserIntel intel = Config.Data.Intel.GetOrCreateUserIntel(userAfter.Id);
            if (intel.ChangeState(userAfter.Status))
                return Config.Data.Intel.SaveDelayedAsync(TimeSpan.FromMinutes(2.5));
            return Task.CompletedTask;
        }

        protected static string GetMaxUserAvatarUrl(IUser user, ImageFormat format = ImageFormat.Auto)
            => GetUserAvatarUrl(user, format, (ushort)(user is SocketUser ? 2048 : 1024));
        protected static string GetUserAvatarUrl(IUser user, ImageFormat format = ImageFormat.Auto, ushort size = 128)
            => user.GetAvatarUrl(format, size) ?? user.GetDefaultAvatarUrl();

        private EmbedBuilder AddUserInfo(EmbedBuilder embed, IUser user)
        {
            string activityString = user.Activity == null ? "-" : $"*{ActivityTypeToString(user.Activity.Type)}* `{user.Activity.Name}`";
            embed.WithAuthor($"Intel on {user.Username}", GetUserAvatarUrl(Client.CurrentUser))
                .WithThumbnailUrl(GetMaxUserAvatarUrl(user))
                .AddField("Username and Discriminator", $"{user.Username}#{user.Discriminator}")
                .AddField("Account age", (DateTimeOffset.UtcNow - user.CreatedAt).ToLongFriendlyString())
                .AddField("Status", (user is SocketUser) ? user.Status.ToString() : "???", true);
            if (user.Activity != null)
                embed.AddField("Activity", activityString, true);
            if (Config.Data.Intel.UserIntel.TryGetValue(user.Id, out UserIntel intel))
            {
                embed.AddField(user.Status == UserStatus.Offline ? "No visual for" : "Online for",
                    (DateTimeOffset.UtcNow - intel.ChangeTimeUTC.Value).ToFriendlyString(), true);
            }
            embed.AddField("User type", user.IsWebhook ? "Webhook" : user.IsBot ? "Bot" : "Normal user")
                .WithTimestamp(user.CreatedAt)
                .WithFooter($"User ID: {user.Id}", GetUserAvatarUrl(user));
            return embed;
        }

        private EmbedBuilder AddGuildUserInfo(EmbedBuilder embed, SocketGuildUser user)
        {
            IOrderedEnumerable<SocketRole> roles = user.Roles.Where(r => r.Id != user.Guild.EveryoneRole.Id).OrderByDescending(r => r.Position);
            if (user.Nickname != null)
                embed.AddField("Guild nickname", user.Nickname, true)
                    .WithAuthor($"Intel on {user.Nickname}", GetUserAvatarUrl(Client.CurrentUser));
            embed.AddField("Roles", string.Join(", ", roles.Select(r => MentionUtils.MentionRole(r.Id))), true);
            if (user.JoinedAt != null)
                embed.AddField("Time in this guild", (DateTimeOffset.UtcNow - user.JoinedAt.Value).ToLongFriendlyString(), true);
            embed.Color = roles.FirstOrDefault(r => r.Color != Color.Default)?.Color;
            return embed;
        }

        private EmbedBuilder AddGuildInfo(EmbedBuilder embed, SocketGuild guild)
        {
            embed.WithAuthor($"Intel on {guild.Name}", GetUserAvatarUrl(Client.CurrentUser))
                .WithThumbnailUrl(guild.IconUrl)
                .AddField("Owner", MentionUtils.MentionUser(guild.OwnerId))
                .AddField("Guild age", (DateTimeOffset.UtcNow - guild.CreatedAt).ToLongFriendlyString(), true)
                .AddField("Members", guild.MemberCount.ToString(), true)
                .AddField("Roles", guild.Roles.Count.ToString(), true)
                .AddField("Channels", guild.Channels.Count.ToString(), true)
                .AddField("Default channel", guild.DefaultChannel.Mention, true);
            if (guild.AFKChannel != null)
            {
                embed.AddField("AFK Channel", MentionUtils.MentionChannel(guild.AFKChannel.Id), true)
                    .AddField("AFK Timeout", TimeSpan.FromSeconds(guild.AFKTimeout).ToShortFriendlyString(), true);
            }
            embed.AddField("Custom emotes", $"{guild.Emotes.Count}: {string.Join(' ', guild.Emotes.Select(e => e.ToString()))}")
                .AddField("Admin 2FA requirement", guild.MfaLevel.ToString(), true)
                .AddField("Verification level", guild.VerificationLevel.ToString(), true)
                .AddField("Default notifications", DefaultMessageNotificationsToString(guild.DefaultMessageNotifications), true)
                .AddField("Explicit content filter", ExplicitContentFilterLevelToString(guild.ExplicitContentFilter), true)
                .WithTimestamp(guild.CreatedAt)
                .WithFooter($"Guild ID: {guild.Id}", guild.IconUrl);
            return embed;
        }

        protected static string DefaultMessageNotificationsToString(DefaultMessageNotifications value)
        {
            switch (value)
            {
                case DefaultMessageNotifications.AllMessages:
                    return "All messages";
                case DefaultMessageNotifications.MentionsOnly:
                    return "Mentions only";
                default:
                    return null;
            }
        }

        protected static string ExplicitContentFilterLevelToString(ExplicitContentFilterLevel value)
        {
            switch (value)
            {
                case ExplicitContentFilterLevel.Disabled:
                    return "Disabled";
                case ExplicitContentFilterLevel.MembersWithoutRoles:
                    return "Filtered for members without roles";
                case ExplicitContentFilterLevel.AllMembers:
                    return "Filtered for all members";
                default:
                    return null;
            }
        }

        protected static string ActivityTypeToString(ActivityType value)
        {
            switch (value)
            {
                case ActivityType.Listening:
                    return "Listening to";
                case ActivityType.Playing:
                    return "Playing";
                case ActivityType.Streaming:
                    return "Streaming";
                case ActivityType.Watching:
                    return "Watching";
                default:
                    return null;
            }
        }
    }
}
