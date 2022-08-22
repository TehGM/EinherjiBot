using TehGM.EinherjiBot.PlaceholdersEngine;

namespace TehGM.EinherjiBot.UI.PlaceholdersEngine
{
    public class PlaceholderBuilderResult
    {
        public string SerializedValue { get; }
        public object PlaceholderObject { get; }
        public PlaceholderDescriptor Descriptor { get; }

        public bool IsSuccess => !string.IsNullOrWhiteSpace(this.SerializedValue);

        private PlaceholderBuilderResult(string serializedValue, object placeholderObject, PlaceholderDescriptor descriptor)
        {
            this.SerializedValue = serializedValue;
            this.PlaceholderObject = placeholderObject;
            this.Descriptor = descriptor;
        }

        public static PlaceholderBuilderResult Success(string serializedValue, object placeholderObject, PlaceholderDescriptor descriptor)
            => new PlaceholderBuilderResult(serializedValue, placeholderObject, descriptor);

        public static PlaceholderBuilderResult Cancelled { get; } = new PlaceholderBuilderResult(null, null, null);
    }
}
