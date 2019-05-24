using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TehGM.EinherjiBot.Extensions;
using TehGM.EinherjiBot.DataModels;
using System.Linq;
using System.Threading;
using TehGM.EinherjiBot.Utilities;

namespace TehGM.EinherjiBot.Config
{
    public class BotDataIntel
    {
        public const string DefaultPath = "Config/data_intel.json";
        public event Action<BotDataIntel> Saving;

        [JsonIgnore]
        public IDictionary<ulong, UserIntel> UserIntel { get; private set; }

        private CancellationTokenSource _delayedSaveCts;

        [JsonConstructor]
        public BotDataIntel(UserIntel[] intelCollection)
        {
            UserIntel = new Dictionary<ulong, UserIntel>(intelCollection == null ? 0 : intelCollection.Length);
            if (intelCollection == null)
                return;
            for (int i = 0; i < intelCollection.Length; i++)
            {
                UserIntel intel = intelCollection[i];
                UserIntel.Add(intel.UserID, intel);
            }
        }

        public UserIntel GetOrCreateUserIntel(ulong userId)
        {
            if (!UserIntel.TryGetValue(userId, out UserIntel result))
            {
                result = new UserIntel(userId);
                UserIntel.Add(userId, result);
            }
            return result;
        }

        public static async Task<BotDataIntel> LoadAsync(string filePath = DefaultPath)
        {
            JToken fileContents = await JsonFileExtensions.LoadFromFileAsync(filePath);
            return fileContents.ToObject<BotDataIntel>();
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

            Saving?.Invoke(this);
            JObject obj = JObject.FromObject(this);
            obj.Add("intelCollection", JToken.FromObject(UserIntel.Values));
            return JsonFileExtensions.SaveToFileAsync((JToken)obj, filePath);
        }
    }
}
