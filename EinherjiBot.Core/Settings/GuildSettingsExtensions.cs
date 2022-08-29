namespace TehGM.EinherjiBot.Settings
{
    public static class GuildSettingsExtensions
    {
        /// <summary>Checks if provided request would make any changes to given guild settings.</summary>
        /// <param name="settings">Existing settings.</param>
        /// <param name="request">Data of new settings state.</param>
        /// <returns>Whether <paramref name="request"/> would change <paramref name="settings"/> in any way.</returns>
        public static bool HasChanges(this IGuildSettings settings, GuildSettingsRequest request)
        {
            return settings.MaxMessageTriggers != request.MaxMessageTriggers
                || settings.JoinNotification.HasChanges(request.JoinNotification)
                || settings.LeaveNotification.HasChanges(request.LeaveNotification);
        }

        /// <summary>Checks if provided request would make any changes to given join/leave notification settings.</summary>
        /// <param name="settings">Existing settings.</param>
        /// <param name="request">Data of new settings state.</param>
        /// <returns>Whether <paramref name="request"/> would change <paramref name="settings"/> in any way.</returns>
        public static bool HasChanges(this IJoinLeaveSettings settings, JoinLeaveSettingsRequest request)
        {
            return settings.IsEnabled != request.IsEnabled
                || settings.UseSystemChannel != request.UseSystemChannel
                || settings.NotificationChannelID != request.NotificationChannelID
                || settings.MessageTemplate != request.MessageTemplate
                || settings.ShowUserAvatar != request.ShowUserAvatar
                || settings.EmbedColor != request.EmbedColor;
        }
    }
}
