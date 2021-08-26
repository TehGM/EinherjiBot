using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;

namespace TehGM.EinherjiBot.Kathara
{
    public class PiholeOptions
    {
        public TimeSpan DefaultDisableTime { get; set; } = TimeSpan.FromMinutes(5);
        public IDictionary<string, PiholeInstanceOptions> Instances { get; set; } = new Dictionary<string, PiholeInstanceOptions>(StringComparer.OrdinalIgnoreCase);

        public bool IsAuthorized(DiscordUser user, string instanceID)
            => this.Instances[instanceID].IsAuthorized(user);

        public IReadOnlyDictionary<string, PiholeInstanceOptions> GetUserAuthorizedInstances(DiscordUser user)
            => this.Instances.Where(kvp => kvp.Value.IsAuthorized(user)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public class PiholeInstanceOptions
    {
        public string DisplayName { get; set; }
        public string PiholeURL { get; set; } = "http://pi.hole/admin/";
        public bool HideURL { get; set; } = false;
        public string AuthToken { get; set; }
        public HashSet<ulong> AuthorizedRoleIDs { get; set; }
        public HashSet<ulong> AuthorizedUserIDs { get; set; }

        public bool IsAuthorized(DiscordUser user)
        {
            if (this.AuthorizedUserIDs.Contains(user.Id))
                return true;
            if (user is DiscordMember guildUser)
                return this.AuthorizedRoleIDs.Intersect(guildUser.Roles.Select(role => role.Id)).Any();
            return false;
        }
    }
}
