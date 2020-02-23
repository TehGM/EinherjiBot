using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace TehGM.EinherjiBot.Config
{
    public class PiholeConfig
    {
        [JsonProperty("defaultDisableMinutes")]
        [DefaultValue(5)]
        public uint DefaultDisableMinutes { get; private set; } = 5;
        [JsonIgnore]
        public IDictionary<string, PiholeInstanceConfig> Instances { get; private set; }

        [JsonConstructor]
        public PiholeConfig(PiholeInstanceConfig[] instances)
        {
            Instances = new Dictionary<string, PiholeInstanceConfig>(instances?.Length ?? 0, StringComparer.OrdinalIgnoreCase);
            if (instances != null)
            {
                foreach (PiholeInstanceConfig inst in instances)
                    Instances.Add(inst.InstanceID, inst);
            }
        }

        public bool IsAuthorized(IUser user, string instanceID)
            => Instances[instanceID].IsAuthorized(user);

        public IEnumerable<PiholeInstanceConfig> UserAuthorizedInstances(IUser user)
            => Instances.Values.Where(instance => instance.IsAuthorized(user));
    }

    public class PiholeInstanceConfig
    {
        [JsonProperty("instanceId", Required = Required.Always)]
        public string InstanceID { get; private set; }
        [JsonProperty("piholeUrl", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("http://pi.hole/admin/")]
        public string PiholeURL { get; private set; }
        [JsonProperty("hideUrl", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(false)]
        public bool HideURL { get; private set; }
        [JsonProperty("token", Required = Required.Always)]
        public string AuthToken { get; private set; }
        [JsonProperty("authorizedRoles")]
        public HashSet<ulong> AuthorizedRolesIDs { get; private set; }
        [JsonProperty("authorizedUsers")]
        public HashSet<ulong> AuthorizedUsersIDs { get; private set; }

        [JsonConstructor]
        public PiholeInstanceConfig(HashSet<ulong> authorizedRoles, HashSet<ulong> authorizedUsers)
        {
            if (authorizedRoles != null)
                AuthorizedRolesIDs = authorizedRoles;
            else AuthorizedRolesIDs = new HashSet<ulong>();
            if (authorizedUsers != null)
                AuthorizedUsersIDs = authorizedUsers;
            else AuthorizedUsersIDs = new HashSet<ulong>();
        }

        public bool IsAuthorized(IUser user)
        {
            if (AuthorizedUsersIDs.Contains(user.Id))
                return true;
            if (user is SocketGuildUser guildUser)
                return AuthorizedRolesIDs.Intersect(guildUser.Roles.Select(role => role.Id)).Any();
            return false;
        }
    }
}
