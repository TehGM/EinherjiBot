using System;
using System.Collections.Generic;

namespace TehGM.EinherjiBot.GameServers
{
    public class GameServersOptions
    {
        public HashSet<ulong> RoleScanGuildIDs { get; set; } = new HashSet<ulong>();
        public TimeSpan AutoRemoveDelay { get; set; } = TimeSpan.FromSeconds(300);
    }
}
