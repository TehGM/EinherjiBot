using Discord;

namespace TehGM.EinherjiBot.Settings
{
    public static class GuildSettingsExtensions
    {
        public static bool IsDefaultFor(this IGuildSettings settings, IGuild guild)
            => settings.GuildID == guild.Id && settings.JoinNotificationChannelID == guild.SystemChannelId && settings.LeaveNotificationChannelID == guild.SystemChannelId;

        /// <summary>Checks if provided request would make any changes to given guild settings.</summary>
        /// <param name="settings">Existing settings.</param>
        /// <param name="request">Data of new settings state.</param>
        /// <returns>Whether <paramref name="request"/> would change <paramref name="settings"/> in any way.</returns>
        public static bool HasChanges(this IGuildSettings settings, GuildSettingsRequest request)
        {
            return settings.JoinNotificationChannelID != request.JoinNotificationChannelID
                || settings.LeaveNotificationChannelID != request.LeaveNotificationChannelID
                || settings.MaxMessageTriggers != request.MaxMessageTriggers;
        }
    }
}
