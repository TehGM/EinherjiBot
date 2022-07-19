using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace TehGM.EinherjiBot.Intel.Commands
{
    [Group("intel", "Get Discord intel reports")]
    [EnabledInDm(true)]
    public class UserIntelSlashCommands : EinherjiInteractionModule
    {
        [Group("on", "Get Discord intel reports")]
        public class UserIntelOnCommands : EinherjiInteractionModule
        {
            private readonly IUserIntelProvider _provider;
            private readonly ILogger _log;

            public UserIntelOnCommands(IUserIntelProvider provider, ILogger<UserIntelSlashCommands> log)
            {
                this._provider = provider;
                this._log = log;
            }

            [SlashCommand("user", "Gets intel on specific user")]
            public async Task CmdIntelUserAsync(
                [Summary("User", "User to get intel on")] IUser user)
            {
                this._log.LogDebug("Building intel on user {Username} ({UserID})", user.GetUsernameWithDiscriminator(), user.Id);

                // global user info
                UserIntel intel = await this._provider.GetAsync(user.Id, base.Context.Guild?.Id, base.CancellationToken).ConfigureAwait(false);
                EmbedBuilder embed = new EmbedBuilder()
                    .WithAuthor($"Intel on {intel.User.Username}", base.Context.Client.CurrentUser.GetSafeAvatarUrl())
                    .WithThumbnailUrl(intel.User.GetMaxAvatarUrl())
                    .AddField("Username and Discriminator", $"{intel.User.Username}#{intel.User.Discriminator}")
                    .AddField("Account created", TimestampTag.FromDateTimeOffset(intel.User.CreatedAt, TimestampTagStyles.Relative))
                    .AddField("Status", (intel.User is SocketUser) ? intel.User.Status.ToString() : "???", true);

                if (user.Activities.Any())
                    embed.AddField("Activity", GetUserActivity(user), true);

                if (intel.LatestStatus != null)
                {
                    embed.AddField(user.Status == UserStatus.Offline ? "No visual since" : "Online since",
                        TimestampTag.FromDateTime(intel.LatestStatus.Timestamp, TimestampTagStyles.Relative), true);
                }

                embed.AddField("User type", intel.User.IsWebhook ? "Webhook" : intel.User.IsBot ? "Bot" : "Normal user")
                    .WithTimestamp(intel.User.CreatedAt)
                    .WithFooter($"User ID: {intel.User.Id}", intel.User.GetSafeAvatarUrl());

                // guild member info
                if (intel.GuildUser != null)
                {
                    if (intel.GuildUser.Nickname != null)
                    {
                        embed.WithAuthor($"Intel on {intel.GuildUser.Nickname}", base.Context.Client.CurrentUser.GetSafeAvatarUrl());
                        embed.AddField("Guild nickname", intel.GuildUser.Nickname, true);
                    }

                    // get roles, respecting hierarchy
                    IOrderedEnumerable<IRole> roles = intel.GuildUser.GetRoles(r => r.Id != base.Context.Guild.EveryoneRole.Id).OrderByDescending(r => r.Position);
                    if (roles.Any())
                        embed.AddField("Roles", string.Join(", ", roles.Select(r => MentionUtils.MentionRole(r.Id))), true);
                    else
                        embed.AddField("Roles", "-");
                    embed.Color = intel.GuildUser.GetHighestRole()?.Color;

                    // add joined time
                    if (intel.GuildUser.JoinedAt != null)
                        embed.AddField("Joined this guild", TimestampTag.FromDateTimeOffset(intel.GuildUser.JoinedAt.Value, TimestampTagStyles.Relative), true);
                }

                await base.RespondAsync(null, embed.Build()).ConfigureAwait(false);
            }

            [SlashCommand("me", "Gets intel on you")]
            public Task CmdIntelMeAsync()
            {
                return this.CmdIntelUserAsync(base.Context.User);
            }

            [SlashCommand("guild", "Gets intel on current guild")]
            [EnabledInDm(false)]
            public async Task CmdIntelGuildAsync()
            {
                EmbedBuilder embed = new EmbedBuilder();
                IGuild guild = base.Context.Guild;

                IEnumerable<IGuildUser> members = await guild.GetUsersAsync(CacheMode.AllowDownload, base.GetRequestOptions()).ConfigureAwait(false);
                IEnumerable<IGuildChannel> channels = await guild.GetChannelsAsync(CacheMode.AllowDownload, base.GetRequestOptions()).ConfigureAwait(false);
                ITextChannel defaultChannel = await guild.GetDefaultChannelAsync(CacheMode.AllowDownload, base.GetRequestOptions()).ConfigureAwait(false);

                embed.WithAuthor($"Intel on {guild.Name}", base.Context.Client.CurrentUser.GetSafeAvatarUrl())
                    .WithThumbnailUrl(guild.IconUrl)
                    .AddField("Owner", MentionUtils.MentionUser(guild.OwnerId))
                    .AddField("Created", TimestampTag.FromDateTimeOffset(guild.CreatedAt, TimestampTagStyles.Relative), true)
                    .AddField("Members", members.Count().ToString(), true)
                    .AddField("Roles", guild.Roles.Count.ToString(), true)
                    .AddField("Channels", channels.Count().ToString(), true)
                    .AddField("Default channel", defaultChannel.Mention, true);
                if (guild.AFKChannelId != null)
                {
                    embed.AddField("AFK Channel", MentionUtils.MentionChannel(guild.AFKChannelId.Value), true)
                        .AddField("AFK Timeout", TimeSpan.FromSeconds(guild.AFKTimeout).ToShortFriendlyString(), true);
                }
                embed.AddField("Custom emotes", $"{guild.Emotes.Count}: {string.Join(' ', guild.Emotes.Select(e => e.ToString()))}")
                    .AddField("Admin 2FA requirement", guild.MfaLevel.ToString(), true)
                    .AddField("Verification level", guild.VerificationLevel.ToString(), true)
                    .AddField("Default notifications", DefaultMessageNotificationsToString(guild.DefaultMessageNotifications), true)
                    .AddField("Explicit content filter", ExplicitContentFilterLevelToString(guild.ExplicitContentFilter), true)
                    .WithTimestamp(guild.CreatedAt)
                    .WithFooter($"Guild ID: {guild.Id}", guild.IconUrl);

                await base.RespondAsync(null, embed.Build()).ConfigureAwait(false);
            }

            protected static string GetUserActivity(IUser user)
            {
                if (!user.Activities.Any())
                    return "-";
                return string.Join('\n', user.Activities.Select(activity =>
                {
                    if (activity is CustomStatusGame customStatus)
                        return $"{customStatus.Emote} {customStatus.State}";
                    return $"*{ActivityTypeToString(activity.Type)}* `{activity.Name}`";
                }));
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
        }
    }
}
