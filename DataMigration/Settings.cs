using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TehGM.EinherjiBot.DataMigration
{
    class Settings
    {
        [JsonProperty("ConnectionString")]
        public string ConnectionString { get; private set; }
        [JsonProperty("intelFilename")]
        public string IntelFileName { get; private set; }
        [JsonProperty("dataFilename")]
        public string DataFileName { get; private set; }
        [JsonProperty("DatabaseName")]
        public string DatabaseName { get; private set; }

        public static Task<Settings> LoadAsync(string filename, CancellationToken cancellationToken = default)
            => JsonFileUtility.LoadFromFileAsync<Settings>(filename, cancellationToken);
    }
}
