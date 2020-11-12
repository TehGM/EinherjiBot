using System.Collections.Generic;

namespace TehGM.EinherjiBot.Administration
{
    public class BotChannelsRedirectionOptions
    {
        public HashSet<ulong> IgnoredChannelIDs { get; set; }
        public HashSet<ulong> IgnoredUserIDs { get; set; }
        public bool IgnoreBots { get; set; } = true;
        public IEnumerable<BotChannelsRedirection> Redirections { get; set; }
    }
}
