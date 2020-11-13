using Newtonsoft.Json;

namespace TehGM.EinherjiBot.DataMigration.Entities.Old
{
    class StellarisModInfo
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; private set; }
        [JsonProperty("url", Required = Required.Always)]
        public string URL { get; private set; }

        public StellarisModInfo(string name, string url)
        {
            this.Name = name;
            this.URL = url;
        }
    }
}
