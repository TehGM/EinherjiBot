namespace TehGM.EinherjiBot.Settings
{
    public interface IGuildSettings
    {
        public ulong GuildID { get; }
        public IJoinLeaveSettings JoinNotification { get; }
        public IJoinLeaveSettings LeaveNotification { get; }
        public uint? MaxMessageTriggers { get; }
    }
}
