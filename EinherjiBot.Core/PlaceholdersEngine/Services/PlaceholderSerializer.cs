using System.Reflection;
using System.Text;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Services
{
    public class PlaceholderSerializer : IPlaceholderSerializer
    {
        public string Serialize(object placeholder)
        {
            if (placeholder == null)
                throw new ArgumentNullException(nameof(placeholder));

            Type placeholderType = placeholder.GetType();
            PlaceholderAttribute identifier = GetPlaceholderAttribute(placeholderType);

            IEnumerable<PropertyInfo> properties = placeholderType.GetProperties(PlaceholderDescriptor.MemberBindingFlags);
            IDictionary<string, string> parameters = new Dictionary<string, string>(properties.Count(), StringComparer.OrdinalIgnoreCase);
            foreach (PropertyInfo property in properties)
            {
                PlaceholderPropertyAttribute propAttribute = property.GetCustomAttribute<PlaceholderPropertyAttribute>(inherit: true);
                if (propAttribute == null)
                    continue;

                object value = property.GetValue(placeholder, PlaceholderDescriptor.MemberBindingFlags, null, null, null);
                if (value != null || propAttribute.IsRequired)
                    parameters.Add(propAttribute.Name, value.ToString());
            }

            StringBuilder builder = new StringBuilder(PlaceholderSymbol.OpenTag);
            builder.Append(identifier.Identifier);
            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                builder.Append(PlaceholderSymbol.ParameterSplitter);
                builder.Append(parameter.Key);
                builder.Append(PlaceholderSymbol.KeyValueSplitter);
                builder.Append(parameter.Value);
            }
            builder.Append(PlaceholderSymbol.CloseTag);
            return builder.ToString();
        }

        public object Deserialize(string placeholderValue, Type placeholderType)
        {
            if (placeholderValue == null)
                throw new ArgumentNullException(nameof(placeholderValue));
            if (placeholderType == null)
                throw new ArgumentNullException(nameof(placeholderType));

            if (!placeholderValue.StartsWith(PlaceholderSymbol.OpenTag, StringComparison.Ordinal) || !placeholderValue.EndsWith(PlaceholderSymbol.CloseTag, StringComparison.Ordinal))
                throw new PlaceholderFormatException($"Placeholder value must be wrapped in '{PlaceholderSymbol.OpenTag}' and '{PlaceholderSymbol.CloseTag}' tags.", placeholderType);
            placeholderValue = placeholderValue[PlaceholderSymbol.OpenTag.Length..^PlaceholderSymbol.CloseTag.Length];

            string[] segments = placeholderValue.Split(PlaceholderSymbol.ParameterSplitter);
            ParseIdentifier(segments, placeholderType);

            IDictionary<string, string> parameters = ParseParameters(segments, placeholderType);
            object result = Activator.CreateInstance(placeholderType, nonPublic: true);

            IEnumerable<PropertyInfo> properties = placeholderType.GetProperties(PlaceholderDescriptor.MemberBindingFlags);
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
                property.SetValue(result, actualValue, PlaceholderDescriptor.MemberBindingFlags, null, null, null);
            }

            return result;
        }

        private static string ParseIdentifier(string[] segments, Type placeholderType)
        {
            PlaceholderAttribute identifier = GetPlaceholderAttribute(placeholderType);
            if (identifier == null)
                throw new InvalidOperationException($"Type {placeholderType.FullName} is not decorated with {nameof(PlaceholderAttribute)}");
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
                int splitterIndex = segment.IndexOf(PlaceholderSymbol.KeyValueSplitter);
                if (splitterIndex < 0)
                    throw new PlaceholderFormatException($"Placeholder's segment '{segment}' contains no value.", placeholderType);
                string key = segment.Remove(splitterIndex);
                if (parameters.ContainsKey(key))
                    throw new PlaceholderFormatException($"Placeholder has duplicated parameter key '{key}'.", placeholderType);

                string value = segment.Substring(splitterIndex + PlaceholderSymbol.KeyValueSplitter.Length);
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
