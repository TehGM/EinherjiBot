using System.Collections.Generic;

namespace TehGM.EinherjiBot.Patchbot
{
    public class PatchbotOptions
    {
        public HashSet<ulong> ChannelIDs { get; set; }
        public HashSet<ulong> PatchbotWebhookIDs { get; set; }
    }
}
