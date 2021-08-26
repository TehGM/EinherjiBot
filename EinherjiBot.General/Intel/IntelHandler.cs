using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TehGM.EinherjiBot.CommandsProcessing;

namespace TehGM.EinherjiBot.Intel
{
    [LoadRegexCommands]
    [PersistentModule(PreInitialize = true)]
    [HelpCategory("General", 99999)]
    public class IntelHandler : IDisposable
    {
        private readonly DiscordClient _client;
        private readonly IUserDataStore _userDataStore;
        private readonly IStatusChecker _statusChecker;
        private readonly IOptionsMonitor<EinherjiOptions> _einherjiOptions;
        private readonly IOptionsMonitor<CommandsOptions> _commandsOptions;
        private readonly CancellationTokenSource _hostCts;
        private readonly ILogger _log;

        public IntelHandler(DiscordClient client, IUserDataStore userDataStore, IStatusChecker statusChecker, ILogger<IntelHandler> log,
            IOptionsMonitor<EinherjiOptions> einherjiOptions, IOptionsMonitor<CommandsOptions> commandsOptions)
        {
            this._client = client;
            this._userDataStore = userDataStore;
            this._statusChecker = statusChecker;
            this._log = log;
            this._einherjiOptions = einherjiOptions;
            this._commandsOptions = commandsOptions;
            this._hostCts = new CancellationTokenSource();

            this._client = client;
            this._client.PresenceUpdated += OnGuildMemberUpdatedAsync;
        }

        [RegexCommand("^intel on me")]
        [Hidden]
        [Priority(200)]
        private Task CmdIntelMeAsync(CommandContext context, CancellationToken cancellationToken = default)
            => ProcessIntelUserAsync(context, context.User, cancellationToken);

        [RegexCommand("^intel on \\\\?<@!?(\\d+)>")]
        [RegexCommand("^intel on (\\d+)")]
        [Hidden]
        [Priority(199)]
        private async Task CmdIntelUserAsync(CommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            string idString = match.Groups[1].Value;
            if (!ulong.TryParse(idString, out ulong id))
            {
                await context.ReplyAsync($"{_einherjiOptions.CurrentValue.FailureSymbol} Could not parse user ID `{idString}`.").ConfigureAwait(false);
                return;
            }
            DiscordUser user = await _client.GetUserAsync(id).ConfigureAwait(false);
            if (user == null)
            {
                await context.ReplyAsync($"{_einherjiOptions.CurrentValue.FailureSymbol} Could not find user with ID `{id}`.").ConfigureAwait(false);
                return;
            }
            await ProcessIntelUserAsync(context, user, cancellationToken).ConfigureAwait(false);
        }

        [RegexCommand("^intel on guild")]
        [Hidden]
        [Priority(198)]
        private Task CmdIntelGuildAsync(CommandContext context)
        {
            if (context.Channel.IsPrivate)
                return context.ReplyAsync($"{_einherjiOptions.CurrentValue.FailureSymbol} This command can only be used in a guild channel.");

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();
            AddGuildInfo(embed, context.Guild);
            return context.ReplyAsync(null, embed.Build());
        }

        [RegexCommand("^intel")]
        [Name("intel")]
        [Summary("Shows help for intel feature.")]
        [Priority(197)]
        private Task CmdIntelHelpAsync(CommandContext context)
        {
            string prefix = _commandsOptions.CurrentValue.Prefix;
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .AddField("Intel Commands",
                    $"**{prefix}intel on me** - get intel on yourself\n" +
                    $"**{prefix}intel on** ***<user ping>*** - get intel on pinged user\n" +
                    $"**{prefix}intel on guild** - *(guild only)* get intel on current guild");
            return context.ReplyAsync(null, embed.Build());
        }

        private async Task ProcessIntelUserAsync(CommandContext context, DiscordUser user, CancellationToken cancellationToken)
        {
            UserData userData = await _userDataStore.GetAsync(user.Id, cancellationToken).ConfigureAwait(false);
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            // get user presence and populate main data
            UserStatus? status = await this._statusChecker.GetStatusAsync(user.Id).ConfigureAwait(false);
            AddUserInfo(embed, user, userData, status);

            // populate guild member data
            if (!context.Channel.IsPrivate)
            {
                DiscordMember guildUser = await context.Guild.GetMemberSafeAsync(user.Id).ConfigureAwait(false);
                if (guildUser != null)
                    AddGuildUserInfo(embed, guildUser);
            }
            await context.ReplyAsync(null, embed.Build()).ConfigureAwait(false);
        }

        private async Task OnGuildMemberUpdatedAsync(DiscordClient client, PresenceUpdateEventArgs e)
        {
            // dsharp+ likes to give null for presence before, so handle that accordingly
            UserStatus before = this.GetSafeStatus(e.PresenceBefore);
            UserStatus after = this.GetSafeStatus(e.PresenceAfter);

            if (before == after)
                return;
            if (before != UserStatus.Offline && after != UserStatus.Offline)
                return;

            _log.LogTrace("Updating intel on user {UserID}", e.UserBefore.Id);
            UserData data = await _userDataStore.GetAsync(e.UserAfter.Id, _hostCts.Token).ConfigureAwait(false);
            if (data.ChangeStatus(after))
                await _userDataStore.SetAsync(data, _hostCts.Token).ConfigureAwait(false);
        }

        private UserStatus GetSafeStatus(DiscordPresence presence)
        {
            if (presence == null)
                return UserStatus.Offline;
            return presence.Status;
        }


        #region Embed Builders
        protected static string GetUserActivity(DiscordUser user)
        {
            if (user.Presence?.Activity == null)
                return "-";
            DiscordCustomStatus customStatus = user.Presence?.Activity?.CustomStatus;
            if (customStatus != null)
                return $"{customStatus.Emoji} {customStatus.Name}";
            return $"*{ActivityTypeToString(user.Presence.Activity.ActivityType)}* `{user.Presence.Activity.Name}`";
        }

        private DiscordEmbedBuilder AddUserInfo(DiscordEmbedBuilder embed, DiscordUser user, UserData userData, UserStatus? status)
        {
            // add basic user info
            embed.WithAuthor($"Intel on {user.Username}", _client.CurrentUser.GetSafeAvatarUrl())
                .WithThumbnail(user.GetSafeAvatarUrl())
                .AddField("Username and Discriminator", $"{user.Username}#{user.Discriminator}")
                .AddField("Account age", (DateTimeOffset.UtcNow - user.CreationTimestamp).ToLongFriendlyString())
                .AddField("Status", status?.ToString() ?? "???", true);

            // if user has some activity, add it as well
            if (user.Presence?.Activity != null)
                embed.AddField("Activity", GetUserActivity(user), true);

            // if user was previously tracked, add data on visibility
            if (userData.StatusChangeTimeUTC != null && status != null)
            {
                embed.AddField(status == UserStatus.Offline ? "No visual for" : "Online for",
                    (DateTimeOffset.UtcNow - userData.StatusChangeTimeUTC.Value).ToFriendlyString(), true);
            }

            // add remaining user info
            embed.AddField("User type", user.IsBot ? "Bot" : "Normal user")
                .WithTimestamp(user.CreationTimestamp)
                .WithFooter($"User ID: {user.Id}", user.GetSafeAvatarUrl());
            return embed;
        }

        private DiscordEmbedBuilder AddGuildUserInfo(DiscordEmbedBuilder embed, DiscordMember user)
        {
            // add nickname if present
            if (user.Nickname != null)
                embed.AddField("Guild nickname", user.Nickname, true)
                    .WithAuthor($"Intel on {user.Nickname}", _client.CurrentUser.GetSafeAvatarUrl());

            // get roles, respecting hierarchy
            IOrderedEnumerable<DiscordRole> roles = user.Roles.Where(r => r.Id != user.Guild.EveryoneRole.Id).OrderByDescending(r => r.Position);
            if (roles.Any())
                embed.AddField("Roles", string.Join(", ", roles.Select(r => Formatter.Mention(r))), true);
            else
                embed.AddField("Roles", "-");
            embed.WithColor(roles.FirstOrDefault(r => r.Color.Value != DiscordColor.None.Value)?.Color ?? DiscordColor.None);

            // add joined time
            embed.AddField("Time in this guild", (DateTimeOffset.UtcNow - user.JoinedAt).ToLongFriendlyString(), true);
            return embed;
        }

        private DiscordEmbedBuilder AddGuildInfo(DiscordEmbedBuilder embed, DiscordGuild guild)
        {
            embed.WithAuthor($"Intel on {guild.Name}", _client.CurrentUser.GetSafeAvatarUrl())
                .WithThumbnail(guild.IconUrl)
                .AddField("Owner", MentionID.User(guild.OwnerId))
                .AddField("Guild age", (DateTimeOffset.UtcNow - guild.CreationTimestamp).ToLongFriendlyString(), true)
                .AddField("Members", guild.MemberCount.ToString(), true)
                .AddField("Roles", guild.Roles.Count.ToString(), true)
                .AddField("Channels", guild.Channels.Count.ToString(), true)
                .AddField("Default channel", guild.GetDefaultChannel().Mention, true);
            if (guild.AfkChannel != null)
            {
                embed.AddField("AFK Channel", MentionID.Channel(guild.AfkChannel.Id), true)
                    .AddField("AFK Timeout", TimeSpan.FromSeconds(guild.AfkTimeout).ToShortFriendlyString(), true);
            }
            embed.AddField("Custom emotes", $"{guild.Emojis.Count}: {string.Join(' ', guild.Emojis.Select(e => e.ToString()))}")
                .AddField("Admin 2FA requirement", guild.MfaLevel.ToString(), true)
                .AddField("Verification level", guild.VerificationLevel.ToString(), true)
                .AddField("Default notifications", DefaultMessageNotificationsToString(guild.DefaultMessageNotifications), true)
                .AddField("Explicit content filter", ExplicitContentFilterLevelToString(guild.ExplicitContentFilter), true)
                .WithTimestamp(guild.CreationTimestamp)
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
                    return value.ToString();
            }
        }

        protected static string ExplicitContentFilterLevelToString(ExplicitContentFilter value)
        {
            switch (value)
            {
                case ExplicitContentFilter.Disabled:
                    return "Disabled";
                case ExplicitContentFilter.MembersWithoutRoles:
                    return "Filtered for members without roles";
                case ExplicitContentFilter.AllMembers:
                    return "Filtered for all members";
                default:
                    return value.ToString();
            }
        }

        protected static string ActivityTypeToString(ActivityType value)
        {
            switch (value)
            {
                case ActivityType.ListeningTo:
                    return "Listening to";
                case ActivityType.Playing:
                    return "Playing";
                case ActivityType.Streaming:
                    return "Streaming";
                case ActivityType.Watching:
                    return "Watching";
                case ActivityType.Custom:
                    return string.Empty;
                default:
                    return value.ToString();
            }
        }
        #endregion

        public void Dispose()
        {
            try { this._hostCts?.Cancel(); } catch { }
            try { this._hostCts?.Dispose(); } catch { }
            try { this._client.PresenceUpdated -= OnGuildMemberUpdatedAsync; } catch { }
        }
    }
}
