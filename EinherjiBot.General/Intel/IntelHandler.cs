using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TehGM.EinherjiBot.CommandsProcessing;

namespace TehGM.EinherjiBot.Intel
{
    [LoadRegexCommands]
    [PersistentModule(PreInitialize = true)]
    public class IntelHandler : IDisposable
    {
        private readonly DiscordSocketClient _client;
        private readonly IUserDataStore _userDataStore;
        private readonly IOptionsMonitor<EinherjiOptions> _einherjiOptions;
        private readonly IOptionsMonitor<CommandsOptions> _commandsOptions;
        private readonly ILogger _log;

        public IntelHandler(DiscordSocketClient client, IUserDataStore userDataStore, ILogger<IntelHandler> log,
            IOptionsMonitor<EinherjiOptions> einherjiOptions, IOptionsMonitor<CommandsOptions> commandsOptions)
        {
            this._client = client;
            this._userDataStore = userDataStore;
            this._log = log;
            this._einherjiOptions = einherjiOptions;
            this._commandsOptions = commandsOptions;

            this._client = client;
            this._client.GuildMemberUpdated += OnGuildMemberUpdatedAsync;
        }

        [RegexCommand("^intel on me")]
        [Priority(200)]
        private Task CmdIntelMeAsync(SocketCommandContext message, CancellationToken cancellationToken = default)
            => ProcessIntelUserAsync(message, message.User, cancellationToken);

        [RegexCommand("^intel on \\\\?<@!?(\\d+)>")]
        [RegexCommand("^intel on (\\d+)")]
        [Priority(199)]
        private async Task CmdIntelUserAsync(SocketCommandContext message, Match match, CancellationToken cancellationToken = default)
        {
            string idString = match.Groups[1].Value;
            if (!ulong.TryParse(idString, out ulong id))
            {
                await message.ReplyAsync($"{_einherjiOptions.CurrentValue.FailureSymbol} Could not parse user ID `{idString}`.", cancellationToken).ConfigureAwait(false);
                return;
            }
            IUser user = await _client.GetUserAsync(id).ConfigureAwait(false);
            if (user == null)
            {
                await message.ReplyAsync($"{_einherjiOptions.CurrentValue.FailureSymbol} Could not find user with ID `{id}`.", cancellationToken).ConfigureAwait(false);
                return;
            }
            await ProcessIntelUserAsync(message, user, cancellationToken);
        }

        [RegexCommand("^intel on guild")]
        [Priority(198)]
        private Task CmdIntelGuildAsync(SocketCommandContext message, CancellationToken cancellationToken = default)
        {
            if (message.IsPrivate)
                return message.ReplyAsync($"{_einherjiOptions.CurrentValue.FailureSymbol} This command can only be used in a guild channel.", cancellationToken);

            EmbedBuilder embed = new EmbedBuilder();
            AddGuildInfo(embed, message.Guild);
            return message.ReplyAsync(null, false, embed.Build(), cancellationToken);
        }

        [RegexCommand("^intel")]
        [Priority(197)]
        private Task CmdIntelHelpAsync(SocketCommandContext message, CancellationToken cancellationToken = default)
        {
            string prefix = _commandsOptions.CurrentValue.Prefix;
            EmbedBuilder embed = new EmbedBuilder()
                .AddField("Intel Commands",
                    $"**{prefix}intel on me** - get intel on yourself\n" +
                    $"**{prefix}intel on** ***<user ping>*** - get intel on pinged user\n" +
                    $"**{prefix}intel on guild** - *(guild only)* get intel on current guild");
            return message.ReplyAsync(null, false, embed.Build(), cancellationToken);
        }

        private async Task ProcessIntelUserAsync(SocketCommandContext message, IUser user, CancellationToken cancellationToken)
        {
            UserData userData = await _userDataStore.GetAsync(user.Id, cancellationToken).ConfigureAwait(false);
            EmbedBuilder embed = new EmbedBuilder();
            AddUserInfo(embed, user, userData);
            if (!message.IsPrivate)
            {
                SocketGuildUser guildUser = message.Guild.GetUser(user.Id);
                if (guildUser != null)
                    AddGuildUserInfo(embed, guildUser);
            }
            await message.ReplyAsync(null, false, embed.Build(), cancellationToken).ConfigureAwait(false);
        }

        private async Task OnGuildMemberUpdatedAsync(SocketGuildUser userBefore, SocketGuildUser userAfter)
        {
            if (userBefore.Status == userAfter.Status)
                return;
            if (userAfter.Status != UserStatus.Offline && userBefore.Status != UserStatus.Offline)
                return;

            UserData data = await _userDataStore.GetAsync(userAfter.Id).ConfigureAwait(false);
            if (data.ChangeStatus(userAfter.Status))
                await _userDataStore.SetAsync(data).ConfigureAwait(false);
        }


        #region Embed Builders
        protected static string GetMaxUserAvatarUrl(IUser user, ImageFormat format = ImageFormat.Auto)
            => GetUserAvatarUrl(user, format, (ushort)(user is SocketUser ? 2048 : 1024));
        protected static string GetUserAvatarUrl(IUser user, ImageFormat format = ImageFormat.Auto, ushort size = 128)
            => user.GetAvatarUrl(format, size) ?? user.GetDefaultAvatarUrl();
        protected static string GetUserActivity(IUser user)
        {
            if (user.Activity == null)
                return "-";
            if (user.Activity.Type == ActivityType.CustomStatus)
                return user.Activity.Details;
            return $"*{ActivityTypeToString(user.Activity.Type)}* `{user.Activity.Name}`";
        }

        private EmbedBuilder AddUserInfo(EmbedBuilder embed, IUser user, UserData userData)
        {
            // add basic user info
            embed.WithAuthor($"Intel on {user.Username}", GetUserAvatarUrl(_client.CurrentUser))
                .WithThumbnailUrl(GetMaxUserAvatarUrl(user))
                .AddField("Username and Discriminator", $"{user.Username}#{user.Discriminator}")
                .AddField("Account age", (DateTimeOffset.UtcNow - user.CreatedAt).ToLongFriendlyString())
                .AddField("Status", (user is SocketUser) ? user.Status.ToString() : "???", true);

            // if user has some activity, add it as well
            if (user.Activity != null)
                embed.AddField("Activity", GetUserActivity(user), true);

            // if user was previously tracked, add data on visibility
            if (userData.StatusChangeTimeUTC != null)
            {
                embed.AddField(user.Status == UserStatus.Offline ? "No visual for" : "Online for",
                    (DateTimeOffset.UtcNow - userData.StatusChangeTimeUTC.Value).ToFriendlyString(), true);
            }

            // add remaining user info
            embed.AddField("User type", user.IsWebhook ? "Webhook" : user.IsBot ? "Bot" : "Normal user")
                .WithTimestamp(user.CreatedAt)
                .WithFooter($"User ID: {user.Id}", GetUserAvatarUrl(user));
            return embed;
        }

        private EmbedBuilder AddGuildUserInfo(EmbedBuilder embed, SocketGuildUser user)
        {
            // add nickname if present
            if (user.Nickname != null)
                embed.AddField("Guild nickname", user.Nickname, true)
                    .WithAuthor($"Intel on {user.Nickname}", GetUserAvatarUrl(_client.CurrentUser));

            // get roles, respecting hierarchy
            IOrderedEnumerable<SocketRole> roles = user.Roles.Where(r => r.Id != user.Guild.EveryoneRole.Id).OrderByDescending(r => r.Position);
            embed.AddField("Roles", string.Join(", ", roles.Select(r => MentionUtils.MentionRole(r.Id))), true);
            embed.Color = roles.FirstOrDefault(r => r.Color != Color.Default)?.Color;

            // add joined time
            if (user.JoinedAt != null)
                embed.AddField("Time in this guild", (DateTimeOffset.UtcNow - user.JoinedAt.Value).ToLongFriendlyString(), true);
            return embed;
        }

        private EmbedBuilder AddGuildInfo(EmbedBuilder embed, SocketGuild guild)
        {
            embed.WithAuthor($"Intel on {guild.Name}", GetUserAvatarUrl(_client.CurrentUser))
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
        #endregion


        #region Enum Converters
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
                case ActivityType.CustomStatus:
                    return string.Empty;
                default:
                    return null;
            }
        }
        #endregion

        public void Dispose()
        {
            try { this._client.GuildMemberUpdated -= OnGuildMemberUpdatedAsync; } catch { }
        }
    }
}
