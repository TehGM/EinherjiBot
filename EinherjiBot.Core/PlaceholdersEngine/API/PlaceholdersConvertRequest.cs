using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace TehGM.EinherjiBot.PlaceholdersEngine.API
{
    public class PlaceholdersConvertRequest
    {
        public const int MaxValueLength = 100;

        [JsonProperty("value", Required = Required.Always)]
        [MaxLength(MaxValueLength)]
        public string Value { get; }

        public PlaceholdersConvertRequest(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            this.Value = value;
        }
    }
}
