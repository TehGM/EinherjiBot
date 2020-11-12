using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TehGM.EinherjiBot.Kathara
{
    class PiholeApiStatusResponse
    {
        [JsonIgnore]
        public bool IsEnabled { get; }
        [JsonIgnore]
        public DateTimeOffset GravityLastUpdated { get; }

        [JsonProperty("domains_being_blocked")]
        public int DomainsBeingBlocked { get; private set; }
        [JsonProperty("dns_queries_today")]
        public int DnsQueriesToday { get; private set; }
        [JsonProperty("ads_blocked_today")]
        public int AdsBlockedToday { get; private set; }
        [JsonProperty("ads_percentage_today")]
        public double AdsPercentageToday { get; private set; }
        [JsonProperty("unique_clients")]
        public int UniqueRecentClients { get; private set; }
        [JsonProperty("clients_ever_seen")]
        public int ClientsEverSeen { get; private set; }
        [JsonProperty("dns_queries_all_types")]
        public int DnsQueriesAllTypes { get; private set; }
        [JsonProperty("reply_NODATA")]
        public int RepliesNODATA { get; private set; }
        [JsonProperty("reply_NXDOMAIN")]
        public int RepliesNXDOMAIN { get; private set; }
        [JsonProperty("reply_CNAME")]
        public int RepliesCNAME { get; private set; }
        [JsonProperty("reply_IP")]
        public int RepliesIP { get; private set; }

        [JsonConstructor]
        private PiholeApiStatusResponse(string status, JObject gravity_last_updated)
        {
            this.IsEnabled = status.Equals("enabled", StringComparison.OrdinalIgnoreCase);
            this.GravityLastUpdated = DateTimeOffset.UnixEpoch.AddSeconds(gravity_last_updated["absolute"].Value<ulong>());
        }
    }
}
