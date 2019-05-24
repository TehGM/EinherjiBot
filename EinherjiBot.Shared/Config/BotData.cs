using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TehGM.EinherjiBot.DataModels;
using TehGM.EinherjiBot.Extensions;

namespace TehGM.EinherjiBot.Config
{
    public class BotData
    {
        public const string DefaultPath = "Config/data.json";

        [JsonProperty("stellarisMods")]
        public List<StellarisModInfo> StellarisMods { get; set; }
        [JsonProperty("netflixAccount")]
        public NetflixAccountInfo NetflixAccount { get; set; }
        [JsonIgnore]
        public BotDataIntel Intel { get; set; }

        private CancellationTokenSource _delayedSaveCts;

        public static async Task<BotData> LoadAsync(string filePath = DefaultPath)
        {
            JToken fileContents = await JsonFileExtensions.LoadFromFileAsync(filePath);
            BotData data = fileContents.ToObject<BotData>();
            data.Intel = await BotDataIntel.LoadAsync();
            return data;
        }

        public async Task SaveDelayedAsync(TimeSpan delay, string filePath = DefaultPath)
        {
            if (_delayedSaveCts != null)
                return;
            _delayedSaveCts = new CancellationTokenSource();
            CancellationToken ct = _delayedSaveCts.Token;
            await Task.Delay(delay, ct);
            if (ct.IsCancellationRequested)
                return;
            await SaveAsync(filePath);
        }

        public Task SaveAsync(string filePath = DefaultPath)
        {
            // cancel any delayed saving if any
            _delayedSaveCts?.Cancel();
            _delayedSaveCts = null;

            Task t1 = JsonFileExtensions.SaveToFileAsync(this, filePath);
            Task t2 = Intel.SaveAsync();
            return Task.WhenAll(t1, t2);
        }
    }
}
