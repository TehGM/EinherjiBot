namespace TehGM.EinherjiBot.DiscordClient
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class GuildCommandAttribute : Attribute
    {
        public ulong[] GuildIDs { get; }

        public GuildCommandAttribute(ulong guildID, params ulong[] otherGuildIDs)
        {
            this.GuildIDs = new ulong[1 + otherGuildIDs.Length];
            this.GuildIDs[0] = guildID;
            Array.Copy(otherGuildIDs, 0, this.GuildIDs, 1, otherGuildIDs.Length);
        }
    }
}
