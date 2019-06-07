using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TehGM.EinherjiBot.Config
{
    public class InaraApiAuth
    {
        [JsonProperty("appName", Required = Required.Always)]
        public string AppName { get; private set; }
        [JsonProperty("appVersion", Required = Required.Always)]
        public string AppVersion { get; private set; }
        [JsonProperty("isDeveloped")]
        public bool IsInDevelopment { get; private set; }
        [JsonProperty("APIkey", Required = Required.Always)]
        public string ApiKey { get; private set; }
        [JsonProperty("commanderName", NullValueHandling = NullValueHandling.Ignore)]
        public string CommanderName { get; private set; }
        [JsonProperty("commanderFrontierID", NullValueHandling = NullValueHandling.Ignore)]
        public string CommanderFrontierID { get; private set; }
    }
}
