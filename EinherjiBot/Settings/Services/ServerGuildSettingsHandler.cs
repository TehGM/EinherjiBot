﻿using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.Auditing;
using TehGM.EinherjiBot.Settings.Policies;
using TehGM.EinherjiBot.Auditing.Settings;

namespace TehGM.EinherjiBot.Settings.Services
{
    public class ServerGuildSettingsHandler : IGuildSettingsHandler
    {
        private readonly IGuildSettingsProvider _provider;
        private readonly IAuthContext _user;
        private readonly IBotAuthorizationService _auth;
        private readonly IAuditStore<GuildSettingsAuditEntry> _audit;
        private readonly ILockProvider _lock;

        public ServerGuildSettingsHandler(IGuildSettingsProvider provider,
            IAuthContext user, IBotAuthorizationService auth, IAuditStore<GuildSettingsAuditEntry> audit, ILockProvider<ServerGuildSettingsHandler> lockProvider)
        {
            this._provider = provider;
            this._user = user;
            this._auth = auth;
            this._audit = audit;
            this._lock = lockProvider;
        }

        public async Task<GuildSettingsResponse> GetAsync(ulong guildID, CancellationToken cancellationToken = default)
        {
            IGuildSettings settings = await this._provider.GetAsync(guildID, cancellationToken).ConfigureAwait(false);
            if (settings == null)
                return null;

            BotAuthorizationResult authorization = await this._auth.AuthorizeAsync(settings, typeof(CanManageGuildSettings), cancellationToken).ConfigureAwait(false);
            if (!authorization.Succeeded)
                throw new AccessForbiddenException(authorization.Reason);

            return this.CreateResponse(settings);
        }

        public async Task<EntityUpdateResult<GuildSettingsResponse>> UpdateAsync(ulong guildID, GuildSettingsRequest request, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                GuildSettings settings = await this._provider.GetAsync(guildID, cancellationToken).ConfigureAwait(false);
                if (settings == null)
                    return null;

                IEnumerable<Type> policies = request.MaxMessageTriggers == settings.MaxMessageTriggers
                    ? new[] { typeof(CanManageGuildSettings) }
                    : new[] { typeof(CanManageGuildSettings), typeof(CanChangeGuildLimits) };
                BotAuthorizationResult authorization = await this._auth.AuthorizeAsync<IGuildSettings>(settings, policies, cancellationToken).ConfigureAwait(false);
                if (!authorization.Succeeded)
                    throw new AccessForbiddenException(authorization.Reason);

                request.ThrowValidateForUpdate(settings);

                if (settings.HasChanges(request))
                {
                    this.ApplyChanges(settings, request);
                    await this._provider.AddOrUpdateAsync(settings, cancellationToken).ConfigureAwait(false);
                    await this._audit.AddAuditAsync(GuildSettingsAuditEntry.Updated(this._user.ID, settings.GuildID, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                    return IEntityUpdateResult.Saved(this.CreateResponse(settings));
                }
                else
                    return IEntityUpdateResult.NoChanges(this.CreateResponse(settings));
            }
            finally
            {
                this._lock.Release();
            }
        }

        private void ApplyChanges(GuildSettings settings, GuildSettingsRequest request)
        {
            settings.JoinNotification = ConvertJoinLeaveSettings(request.JoinNotification);
            settings.LeaveNotification = ConvertJoinLeaveSettings(request.LeaveNotification);
            settings.MaxMessageTriggers = request.MaxMessageTriggers;

            JoinLeaveSettings ConvertJoinLeaveSettings(JoinLeaveSettingsRequest r)
            {
                if (r == null)
                    return null;

                return new JoinLeaveSettings(r.MessageTemplate)
                {
                    IsEnabled = r.IsEnabled,
                    UseSystemChannel = r.UseSystemChannel,
                    NotificationChannelID = r.NotificationChannelID,
                    ShowUserAvatar = r.ShowUserAvatar,
                    EmbedColor = r.EmbedColor,
                    LastError = null
                };
            }
        }

        private GuildSettingsResponse CreateResponse(IGuildSettings settings)
        {
            return new GuildSettingsResponse(settings.GuildID, ConvertJoinLeaveSettings(settings.JoinNotification), ConvertJoinLeaveSettings(settings.LeaveNotification), settings.MaxMessageTriggers);

            JoinLeaveSettingsResponse ConvertJoinLeaveSettings(IJoinLeaveSettings s)
            {
                if (s == null)
                    return null;

                return new JoinLeaveSettingsResponse(
                    s.IsEnabled,
                    s.UseSystemChannel,
                    s.NotificationChannelID,
                    s.MessageTemplate,
                    s.ShowUserAvatar,
                    s.EmbedColor)
                {
                    LastError = ErrorInfoResponse.FromError(s.LastError)
                };
            }
        }
    }
}