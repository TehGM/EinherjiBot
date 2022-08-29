namespace TehGM.EinherjiBot.Settings
{
    public interface IGuildSettings
    {
        public ulong GuildID { get; }
        public ulong? JoinNotificationChannelID { get; }
        public ulong? LeaveNotificationChannelID { get; }
        public uint? MaxMessageTriggers { get; }
    }
}
