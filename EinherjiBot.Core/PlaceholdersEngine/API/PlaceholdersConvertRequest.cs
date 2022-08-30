using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace TehGM.EinherjiBot.PlaceholdersEngine.API
{
    public class PlaceholdersConvertRequest
    {
        public const int MaxValueLength = 400;

        [JsonProperty("value", Required = Required.Always)]
        [MaxLength(MaxValueLength)]
        public string Value { get; set; }
        [JsonProperty("context", Required = Required.Always)]
        public PlaceholderConvertContext Context { get; set; }

        [JsonConstructor]
        private PlaceholdersConvertRequest() { }

        public PlaceholdersConvertRequest(string value, PlaceholderConvertContext context)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            this.Value = value;
            this.Context = context;
        }

        public PlaceholdersConvertRequest(string value, PlaceholderUsage context)
            : this(value, new PlaceholderConvertContext(context)) { }
    }
}
