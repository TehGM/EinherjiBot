using Discord;

namespace TehGM.EinherjiBot.Settings
{
    public interface IJoinLeaveSettings
    {
        bool IsEnabled { get; }
        bool UseSystemChannel { get; }
        ulong? NotificationChannelID { get; }
        string MessageTemplate { get; }
        bool ShowUserAvatar { get; }
        Color EmbedColor { get; }
    }
}
