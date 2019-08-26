using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TehGM.EinherjiBot.DataModels;

namespace TehGM.EinherjiBot.Config
{
    public class PatchbotHelperData
    {
        [JsonProperty("games")]
        public List<PatchbotHelperGame> Games { get; private set; }
        [JsonProperty("patchbotIds")]
        public HashSet<ulong> PatchbotIDs { get; private set; }
        [JsonProperty("webhookIds")]
        public HashSet<ulong> WebhookIDs { get; private set; }

        [JsonConstructor]
        public PatchbotHelperData(List<PatchbotHelperGame> games)
        {
            this.Games = games ?? new List<PatchbotHelperGame>();
        }

        public PatchbotHelperGame FindGame(string nameOrAlias)
        {
            if (string.IsNullOrWhiteSpace(nameOrAlias))
                return null;
            if (Games == null || Games.Count == 0)
                return null;
            string searchTrimmed = nameOrAlias.Trim();
            for (int i = 0; i < Games.Count; i++)
            {
                PatchbotHelperGame game = Games[i];
                if (string.Equals(game.Name, searchTrimmed, StringComparison.OrdinalIgnoreCase))
                    return game;
                if (game.Aliases == null)
                    continue;
                for (int ii = 0; ii < game.Aliases.Count; ii++)
                {
                    if (string.Equals(game.Aliases[ii], searchTrimmed, StringComparison.OrdinalIgnoreCase))
                        return game;
                }
            }
            return null;
        }

        public bool AddPatchbotID(ulong id)
        {
            if (PatchbotIDs == null)
                PatchbotIDs = new HashSet<ulong>();
            return PatchbotIDs.Add(id);
        }

        public bool RemovePatchbotID(ulong id)
        {
            if (PatchbotIDs == null)
                return false;
            return PatchbotIDs.Remove(id);
        }

        public bool AddWebhookID(ulong id)
        {
            if (WebhookIDs == null)
                PatchbotIDs = new HashSet<ulong>();
            return PatchbotIDs.Add(id);
        }

        public bool RemoveWebhookID(ulong id)
        {
            if (WebhookIDs == null)
                return false;
            return WebhookIDs.Remove(id);
        }
    }
}
