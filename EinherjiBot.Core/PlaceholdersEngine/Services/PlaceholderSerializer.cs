using System.Reflection;
using System.Text;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Services
{
    public class PlaceholderSerializer : IPlaceholderSerializer
    {
        public const string OpenTag = "{{";
        public const string CloseTag = "}}";
        public const string ParameterSplitter = "||";
        public const string KeyValueSplitter = "==";

        private const BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public string Serialize(object placeholder)
        {
            if (placeholder == null)
                throw new ArgumentNullException(nameof(placeholder));

            Type placeholderType = placeholder.GetType();
            PlaceholderAttribute identifier = GetPlaceholderAttribute(placeholderType);

            IEnumerable<PropertyInfo> properties = placeholderType.GetProperties(_bindingFlags);
            IDictionary<string, string> parameters = new Dictionary<string, string>(properties.Count(), StringComparer.OrdinalIgnoreCase);
            foreach (PropertyInfo property in properties)
            {
                PlaceholderPropertyAttribute propAttribute = property.GetCustomAttribute<PlaceholderPropertyAttribute>(inherit: true);
                if (propAttribute == null)
                    continue;

                object value = property.GetValue(placeholder, _bindingFlags, null, null, null);
                if (value != null || propAttribute.IsRequired)
                    parameters.Add(propAttribute.Name, value.ToString());
            }

            StringBuilder builder = new StringBuilder(OpenTag);
            builder.Append(identifier.Identifier);
            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                builder.Append(ParameterSplitter);
                builder.Append(parameter.Key);
                builder.Append(KeyValueSplitter);
                builder.Append(parameter.Value);
            }
            builder.Append(CloseTag);
            return builder.ToString();
        }

        public object Deserialize(string placeholderValue, Type placeholderType)
        {
            if (placeholderValue == null)
                throw new ArgumentNullException(nameof(placeholderValue));
            if (placeholderType == null)
                throw new ArgumentNullException(nameof(placeholderType));

            if (!placeholderValue.StartsWith(OpenTag, StringComparison.Ordinal) || !placeholderValue.EndsWith(CloseTag, StringComparison.Ordinal))
                throw new PlaceholderFormatException($"Placeholder value must be wrapped in '{OpenTag}' and '{CloseTag}' tags.", placeholderType);
            placeholderValue = placeholderValue[OpenTag.Length..^CloseTag.Length];

            string[] segments = placeholderValue.Split(ParameterSplitter);
            ParseIdentifier(segments, placeholderType);

            IDictionary<string, string> parameters = ParseParameters(segments, placeholderType);
            object result = Activator.CreateInstance(placeholderType, nonPublic: true);

            IEnumerable<PropertyInfo> properties = placeholderType.GetProperties(_bindingFlags);
            foreach (PropertyInfo property in properties)
            {
                PlaceholderPropertyAttribute propAttribute = property.GetCustomAttribute<PlaceholderPropertyAttribute>(inherit: true);
                if (propAttribute == null)
                    continue;

                if (!parameters.TryGetValue(propAttribute.Name, out string value) && propAttribute.IsRequired)
                    throw new PlaceholderFormatException($"Placeholder's parameter '{propAttribute.Name} is missing.");

                object actualValue = property.PropertyType.IsEnum
                    ? Enum.Parse(property.PropertyType, value, ignoreCase: true)
                    : Convert.ChangeType(value, property.PropertyType);
                property.SetValue(result, actualValue, _bindingFlags, null, null, null);
            }

            return result;
        }

        private static string ParseIdentifier(string[] segments, Type placeholderType)
        {
            PlaceholderAttribute identifier = GetPlaceholderAttribute(placeholderType);
            string name = segments[0];
            if (!identifier.Identifier.Equals(name, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Placeholder identifier mismatch.");
            return identifier.Identifier;
        }

        private static IDictionary<string, string> ParseParameters(string[] segments, Type placeholderType)
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>(segments.Length - 1, StringComparer.OrdinalIgnoreCase);
            foreach (string segment in segments[1..])
            {
                int splitterIndex = segment.IndexOf(KeyValueSplitter);
                if (splitterIndex < 0)
                    throw new PlaceholderFormatException($"Placeholder's segment '{segment}' contains no value.", placeholderType);
                string key = segment.Remove(splitterIndex);
                if (parameters.ContainsKey(key))
                    throw new PlaceholderFormatException($"Placeholder has duplicated parameter key '{key}'.", placeholderType);

                string value = segment.Substring(splitterIndex + KeyValueSplitter.Length);
                if (string.IsNullOrWhiteSpace(value))
                    throw new PlaceholderFormatException($"Placeholder's key '{key}' contains no value.", placeholderType);
            }
            return parameters;
        }

        private static PlaceholderAttribute GetPlaceholderAttribute(Type type)
        {
            PlaceholderAttribute result = type.GetCustomAttribute<PlaceholderAttribute>(inherit: false);
            if (result == null)
                throw new InvalidOperationException($"{type.FullName} isn't decorated with {nameof(PlaceholderAttribute)}.");
            return result;
        }
    }
}
