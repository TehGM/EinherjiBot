﻿using Discord;
using TehGM.EinherjiBot.API.Policies;
using TehGM.EinherjiBot.DiscordClient;

namespace TehGM.EinherjiBot.API.Services
{
    public class ServerDiscordEntityInfoProvider : IDiscordEntityInfoProvider
    {
        private readonly IDiscordClient _client;
        private readonly IDiscordConnection _connection;
        private readonly IBotAuthorizationService _auth;

        public ServerDiscordEntityInfoProvider(IDiscordClient client, IDiscordConnection connection, IBotAuthorizationService auth)
        {
            this._client = client;
            this._connection = connection;
            this._auth = auth;
        }

        // user
        /// <inheritdoc/>
        public async ValueTask<UserInfoResponse> GetBotInfoAsync(CancellationToken cancellationToken = default)
        {
            await this._connection.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
            return CreateUserInfo(this._client.CurrentUser);
        }

        /// <inheritdoc/>
        public async Task<UserInfoResponse> GetUserInfoAsync(ulong userID, CancellationToken cancellationToken = default)
        {
            await this._connection.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
            IUser user = await this._client.GetUserAsync(userID, cancellationToken).ConfigureAwait(false);
            if (user == null)
                return null;
            return CreateUserInfo(user);
        }

        // role
        /// <inheritdoc/>
        public async Task<RoleInfoResponse> GetRoleInfoAsync(ulong roleID, IEnumerable<ulong> guildIDs, CancellationToken cancellationToken = default)
        {
            if (guildIDs != null && !guildIDs.Any())
                return null;

            await this._connection.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
            IEnumerable<IGuild> guilds = await this._client.GetGuildsAsync(CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            if (guildIDs != null)
                guilds = guilds.Where(g => guildIDs.Contains(g.Id));

            foreach (IGuild guild in guilds)
            {
                IRole role = guild.GetRole(roleID);
                if (role == null)
                    continue;

                return CreateRoleInfo(role);
            }
            return null;
        }

        // guild
        /// <inheritdoc/>
        public async Task<IEnumerable<GuildInfoResponse>> GetGuildInfosAsync(IEnumerable<ulong> ids, CancellationToken cancellationToken = default)
        {
            if (ids != null && !ids.Any())
                return null;

            await this._connection.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
            IEnumerable<IGuild> guilds = await this._client.GetGuildsAsync(CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            if (ids != null)
                guilds = guilds.Where(g => ids.Contains(g.Id));


            List<GuildInfoResponse> results = new List<GuildInfoResponse>(guilds.Count());
            foreach (IGuild guild in guilds)
            {
                BotAuthorizationResult authorization = await this._auth.AuthorizeAsync(guild, typeof(CanAccessGuildInfo), cancellationToken).ConfigureAwait(false);
                if (!authorization.Succeeded)
                {
                    // only throw if ids is not null, cause it means client explicitly requested guilds that user has no permissions to
                    // if it is null, it means that client simply requested guilds that user can view - so we just skip those that they can't
                    if (ids != null)
                        throw new AccessForbiddenException($"You have no access to Discord guild {guild.Id}");
                    continue;
                }

                Task<IReadOnlyCollection<IGuildChannel>> channelsTask = guild.GetChannelsAsync(CacheMode.AllowDownload, cancellationToken.ToRequestOptions());
                Task<IReadOnlyCollection<IGuildUser>> usersTask = guild.GetUsersAsync(CacheMode.AllowDownload, cancellationToken.ToRequestOptions());
                IEnumerable<RoleInfoResponse> roles = guild.Roles.Select(CreateRoleInfo);
                IEnumerable<GuildUserInfoResponse> users = usersTask.Result.Select(u => CreateGuildUserInfo(u, roles.IntersectBy(u.RoleIds, r => r.ID)));
                await channelsTask.ConfigureAwait(false);
                List<ChannelInfoResponse> channels = new List<ChannelInfoResponse>(channelsTask.Result.Count);
                foreach (IChannel channel in channelsTask.Result)
                {
                    authorization = await this._auth.AuthorizeAsync(channel, typeof(CanAccessChannelInfo), cancellationToken).ConfigureAwait(false);
                    if (authorization.Succeeded)
                        channels.Add(CreateChannelInfo(channel));
                }

                await usersTask.ConfigureAwait(false);

                GuildInfoResponse result = new GuildInfoResponse(guild.Id, guild.Name, guild.IconId)
                {
                    Users = users,
                    Roles = roles,
                    Channels = channels,
                    OwnerID = guild.OwnerId
                };
                results.Add(result);
            }

            return results.ToArray();
        }

        // guild user
        /// <inheritdoc/>
        public async Task<GuildUserInfoResponse> GetGuildUserInfoAsync(ulong userID, ulong guildID, CancellationToken cancellationToken = default)
        {
            await this._connection.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
            IGuild guild = await this._client.GetGuildAsync(guildID, cancellationToken).ConfigureAwait(false);
            if (guild == null)
                return null;

            BotAuthorizationResult authorization = await this._auth.AuthorizeAsync(guild, typeof(CanAccessGuildInfo), cancellationToken).ConfigureAwait(false);
            if (!authorization.Succeeded)
                throw new AccessForbiddenException(authorization.Reason);

            IGuildUser user = await guild.GetUserAsync(userID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            if (user == null)
                return null;
            IEnumerable<RoleInfoResponse> roles = guild.Roles.IntersectBy(user.RoleIds, r => r.Id).Select(CreateRoleInfo);
            return CreateGuildUserInfo(user, roles);
        }

        public async Task<ChannelInfoResponse> GetChannelInfoAsync(ulong channelID, IEnumerable<ulong> guildIDs, CancellationToken cancellationToken = default)
        {
            if (guildIDs != null && !guildIDs.Any())
                return null;

            await this._connection.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
            IEnumerable<IGuild> guilds = await this._client.GetGuildsAsync(CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            if (guildIDs != null)
                guilds = guilds.Where(g => guildIDs.Contains(g.Id));

            foreach (IGuild guild in guilds)
            {
                IChannel channel = await guild.GetChannelAsync(channelID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
                if (channel == null)
                    continue;

                BotAuthorizationResult authorization = await this._auth.AuthorizeAsync(channel, typeof(CanAccessChannelInfo), cancellationToken).ConfigureAwait(false);
                if (!authorization.Succeeded)
                    throw new AccessForbiddenException(authorization.Reason);

                return CreateChannelInfo(channel);
            }
            return null;
        }

        private static UserInfoResponse CreateUserInfo(IUser user)
            => new UserInfoResponse(user.Id, user.Username, user.Discriminator, user.AvatarId)
            {
                IsBot = user.IsBot || user.IsWebhook
            };
        private static RoleInfoResponse CreateRoleInfo(IRole role)
            => new RoleInfoResponse(role.Id, role.Name, role.Guild.Id, role.Guild.Name, role.Color, role.Position);
        private static ChannelInfoResponse CreateChannelInfo(IChannel channel)
        {
            return new ChannelInfoResponse(channel.Id, channel.Name, channel.GetChannelType().Value, (channel as IGuildChannel)?.GuildId)
            {
                GuildName = (channel as IGuildChannel)?.Guild.Name,
                Topic = (channel as ITextChannel)?.Topic,
                ParentID = (channel as INestedChannel)?.CategoryId,
                Position = (channel as IGuildChannel)?.Position ?? 0
            };
        }
        private static GuildUserInfoResponse CreateGuildUserInfo(IGuildUser user, IEnumerable<RoleInfoResponse> roles)
            => new GuildUserInfoResponse(user.Id, user.Username, user.Discriminator, user.AvatarId, user.GuildId, roles)
            {
                GuildAvatarHash = user.GuildAvatarId,
                Nickname = user.Nickname,
                IsBot = user.IsBot || user.IsWebhook,
                IsAdmin = user.GuildPermissions.Administrator,
                IsOwner = user.Guild.OwnerId == user.Id
            };
    }
}