﻿using System;
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
    [HelpCategory("General", 99999)]
    public class IntelHandler : IDisposable
    {
        private readonly DiscordSocketClient _client;
        private readonly IUserDataStore _userDataStore;
        private readonly IOptionsMonitor<EinherjiOptions> _einherjiOptions;
        private readonly IOptionsMonitor<CommandsOptions> _commandsOptions;
        private readonly CancellationTokenSource _hostCts;
        private readonly ILogger _log;

        public IntelHandler(DiscordSocketClient client, IUserDataStore userDataStore, ILogger<IntelHandler> log,
            IOptionsMonitor<EinherjiOptions> einherjiOptions, IOptionsMonitor<CommandsOptions> commandsOptions)
        {
            this._client = client;
            this._userDataStore = userDataStore;
            this._log = log;
            this._einherjiOptions = einherjiOptions;
            this._commandsOptions = commandsOptions;
            this._hostCts = new CancellationTokenSource();

            this._client = client;
            this._client.GuildMemberUpdated += OnGuildMemberUpdatedAsync;
        }

        [RegexCommand("^intel on me")]
        [Hidden]
        [Priority(200)]
        private Task CmdIntelMeAsync(SocketCommandContext context, CancellationToken cancellationToken = default)
            => ProcessIntelUserAsync(context, context.User, cancellationToken);

        [RegexCommand("^intel on \\\\?<@!?(\\d+)>")]
        [RegexCommand("^intel on (\\d+)")]
        [Hidden]
        [Priority(199)]
        private async Task CmdIntelUserAsync(SocketCommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = _log.BeginCommandScope(context, this);
            string idString = match.Groups[1].Value;
            if (!ulong.TryParse(idString, out ulong id))
            {
                await context.ReplyAsync($"{_einherjiOptions.CurrentValue.FailureSymbol} Could not parse user ID `{idString}`.", cancellationToken).ConfigureAwait(false);
                return;
            }
            IUser user = await _client.GetUserAsync(id).ConfigureAwait(false);
            if (user == null)
            {
                await context.ReplyAsync($"{_einherjiOptions.CurrentValue.FailureSymbol} Could not find user with ID `{id}`.", cancellationToken).ConfigureAwait(false);
                return;
            }
            await ProcessIntelUserAsync(context, user, cancellationToken).ConfigureAwait(false);
        }

        [RegexCommand("^intel on guild")]
        [Hidden]
        [Priority(198)]
        private Task CmdIntelGuildAsync(SocketCommandContext context, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = _log.BeginCommandScope(context, this);
            if (context.IsPrivate)
                return context.ReplyAsync($"{_einherjiOptions.CurrentValue.FailureSymbol} This command can only be used in a guild channel.", cancellationToken);

            EmbedBuilder embed = new EmbedBuilder();
            AddGuildInfo(embed, context.Guild);
            return context.ReplyAsync(null, false, embed.Build(), cancellationToken);
        }

        [RegexCommand("^intel")]
        [Name("intel")]
        [Summary("Shows help for intel feature.")]
        [Priority(197)]
        private Task CmdIntelHelpAsync(SocketCommandContext context, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = _log.BeginCommandScope(context, this);
            string prefix = _commandsOptions.CurrentValue.Prefix;
            EmbedBuilder embed = new EmbedBuilder()
                .AddField("Intel Commands",
                    $"**{prefix}intel on me** - get intel on yourself\n" +
                    $"**{prefix}intel on** ***<user ping>*** - get intel on pinged user\n" +
                    $"**{prefix}intel on guild** - *(guild only)* get intel on current guild");
            return context.ReplyAsync(null, false, embed.Build(), cancellationToken);
        }

        private async Task ProcessIntelUserAsync(SocketCommandContext context, IUser user, CancellationToken cancellationToken)
        {
            UserData userData = await _userDataStore.GetAsync(user.Id, cancellationToken).ConfigureAwait(false);
            EmbedBuilder embed = new EmbedBuilder();
            AddUserInfo(embed, user, userData);
            if (!context.IsPrivate)
            {
                SocketGuildUser guildUser = await context.Guild.GetGuildUserAsync(user.Id).ConfigureAwait(false);
                if (guildUser != null)
                    AddGuildUserInfo(embed, guildUser);
            }
            await context.ReplyAsync(null, false, embed.Build(), cancellationToken).ConfigureAwait(false);
        }

        private async Task OnGuildMemberUpdatedAsync(SocketGuildUser userBefore, SocketGuildUser userAfter)
        {
            if (userBefore.Status == userAfter.Status)
                return;
            if (userAfter.Status != UserStatus.Offline && userBefore.Status != UserStatus.Offline)
                return;

            _log.LogTrace("Updating intel on user {UserID}", userBefore.Id);
            UserData data = await _userDataStore.GetAsync(userAfter.Id, _hostCts.Token).ConfigureAwait(false);
            if (data.ChangeStatus(userAfter.Status))
                await _userDataStore.SetAsync(data, _hostCts.Token).ConfigureAwait(false);
        }


        #region Embed Builders
        protected static string GetUserActivity(IUser user)
        {
            if (user.Activity == null)
                return "-";
            if (user.Activity is CustomStatusGame customStatus)
                return $"{customStatus.Emote} {customStatus.State}";
            return $"*{ActivityTypeToString(user.Activity.Type)}* `{user.Activity.Name}`";
        }

        private EmbedBuilder AddUserInfo(EmbedBuilder embed, IUser user, UserData userData)
        {
            // add basic user info
            embed.WithAuthor($"Intel on {user.Username}", _client.CurrentUser.GetSafeAvatarUrl())
                .WithThumbnailUrl(user.GetMaxAvatarUrl())
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
                .WithFooter($"User ID: {user.Id}", user.GetSafeAvatarUrl());
            return embed;
        }

        private EmbedBuilder AddGuildUserInfo(EmbedBuilder embed, SocketGuildUser user)
        {
            // add nickname if present
            if (user.Nickname != null)
                embed.AddField("Guild nickname", user.Nickname, true)
                    .WithAuthor($"Intel on {user.Nickname}", _client.CurrentUser.GetSafeAvatarUrl());

            // get roles, respecting hierarchy
            IOrderedEnumerable<SocketRole> roles = user.Roles.Where(r => r.Id != user.Guild.EveryoneRole.Id).OrderByDescending(r => r.Position);
            if (roles.Any())
                embed.AddField("Roles", string.Join(", ", roles.Select(r => MentionUtils.MentionRole(r.Id))), true);
            else
                embed.AddField("Roles", "-");
            embed.Color = roles.FirstOrDefault(r => r.Color != Color.Default)?.Color;

            // add joined time
            if (user.JoinedAt != null)
                embed.AddField("Time in this guild", (DateTimeOffset.UtcNow - user.JoinedAt.Value).ToLongFriendlyString(), true);
            return embed;
        }

        private EmbedBuilder AddGuildInfo(EmbedBuilder embed, SocketGuild guild)
        {
            embed.WithAuthor($"Intel on {guild.Name}", _client.CurrentUser.GetSafeAvatarUrl())
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
                    return value.ToString();
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
                    return value.ToString();
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
                    return value.ToString();
            }
        }
        #endregion

        public void Dispose()
        {
            try { this._hostCts?.Cancel(); } catch { }
            try { this._hostCts?.Dispose(); } catch { }
            try { this._client.GuildMemberUpdated -= OnGuildMemberUpdatedAsync; } catch { }
        }
    }
}
