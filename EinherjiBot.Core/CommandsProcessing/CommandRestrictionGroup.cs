using System.Collections.Generic;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    public class CommandRestrictionGroup
    {
        public static string MainGuild => "MainGuild";

        public ICollection<ulong> GuildIDs { get; set; } = new HashSet<ulong>();
    }
}
