using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TehGM.EinherjiBot.DataModels;

namespace TehGM.EinherjiBot.Config
{
    public class PatchbotHelperData
    {
        [JsonProperty("games", Required = Required.Always)]
        public PatchbotHelperGame[] Games { get; private set; }
        [JsonProperty("patchbotIds")]
        public HashSet<ulong> PatchbotIDs { get; private set; }

        public PatchbotHelperGame FindGame(string nameOrAlias)
        {
            if (Games == null || Games.Length == 0)
                return null;
            for (int i = 0; i < Games.Length; i++)
            {
                PatchbotHelperGame game = Games[i];
                if (string.Equals(game.Name, nameOrAlias, StringComparison.OrdinalIgnoreCase))
                    return game;
                if (game.Aliases == null)
                    continue;
                for (int ii = 0; i < game.Aliases.Length; ii++)
                {
                    if (string.Equals(game.Aliases[ii], nameOrAlias, StringComparison.OrdinalIgnoreCase))
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
    }
}
