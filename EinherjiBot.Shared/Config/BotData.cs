using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TehGM.EinherjiBot.DataModels;
using TehGM.EinherjiBot.DataModels.Permits;
using TehGM.EinherjiBot.Extensions;
using TehGM.EinherjiBot.Utilities;

namespace TehGM.EinherjiBot.Config
{
    public class BotData
    {
        public const string DefaultPath = "Config/data.json";

        [JsonProperty("stellarisMods")]
        public List<StellarisModInfo> StellarisMods { get; set; }
        [JsonProperty("netflixAccount")]
        public NetflixPermitInfo NetflixAccount { get; set; }
        [JsonIgnore]
        public BotDataIntel Intel { get; set; }
        [JsonProperty("eliteApi")]
        public EliteApiData EliteAPI { get; set; }
        [JsonProperty("patchbotHelper")]
        public PatchbotHelperData PatchbotHelper { get; set; }

        [JsonIgnore]
        private readonly AsyncDelayedInvoker SaveDataInvoker = new AsyncDelayedInvoker();

        public static async Task<BotData> LoadAsync(string filePath = DefaultPath)
        {
            JToken fileContents = await JsonFileExtensions.LoadFromFileAsync(filePath);
            BotData data = fileContents.ToObject<BotData>();
            data.Intel = await BotDataIntel.LoadAsync();
            data.CreateObjectsIfMissing();
            return data;
        }

        public Task SaveDelayedAsync(TimeSpan delay, string filePath = DefaultPath)
            => SaveDataInvoker.InvokeDelayedAsync(delay, () => SaveInternalAsync(filePath));

        public Task SaveAsync(string filePath = DefaultPath)
            => SaveDataInvoker.InvokeNowAsync(() => SaveInternalAsync(filePath));

        private Task SaveInternalAsync(string filePath = DefaultPath)
        {
            Task t1 = JsonFileExtensions.SaveToFileAsync(this, filePath);
            Task t2 = Intel.SaveAsync();
            return Task.WhenAll(t1, t2);
        }

        private void CreateObjectsIfMissing()
        {
            if (EliteAPI == null)
                EliteAPI = new EliteApiData(null);
            if (NetflixAccount == null)
                NetflixAccount = new NetflixPermitInfo();
            if (StellarisMods == null)
                StellarisMods = new List<StellarisModInfo>();
            if (Intel == null)
                Intel = new BotDataIntel(null);
            if (PatchbotHelper == null)
                PatchbotHelper = new PatchbotHelperData(null);
        }
    }
}
