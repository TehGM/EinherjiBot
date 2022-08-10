using System.Reflection;
using System.Text.RegularExpressions;

namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    public class PlaceholderDescriptor
    {
        public const BindingFlags MemberBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public Type PlaceholderType { get; }
        public PlaceholderAttribute PlaceholderAttribute { get; }
        public Type HandlerType { get; }
        public IEnumerable<PlaceholderPropertyDescriptor> Properties { get; }

        public string Identifier => this.PlaceholderAttribute.Identifier;
        public Regex MatchingRegex => this.PlaceholderAttribute.MatchingRegex;

        public PlaceholderDescriptor(Type type, Type handlerType)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (handlerType == null)
                throw new ArgumentNullException(nameof(handlerType));

            PlaceholderAttribute attribute = type.GetCustomAttribute<PlaceholderAttribute>(inherit: false);
            if (attribute == null)
                throw new ArgumentException($"Property {type} isn't decorated with {nameof(PlaceholderAttribute)}.");

            this.PlaceholderType = type;
            this.PlaceholderAttribute = attribute;
            this.HandlerType = handlerType;
            this.Properties = LoadProperties(type);
        }

        private static IEnumerable<PlaceholderPropertyDescriptor> LoadProperties(Type type)
        {
            IEnumerable<PropertyInfo> properties = type.GetProperties(MemberBindingFlags);
            List<PlaceholderPropertyDescriptor> results = new List<PlaceholderPropertyDescriptor>(properties.Count());
            foreach (PropertyInfo property in properties)
            {
                PlaceholderPropertyAttribute propAttribute = property.GetCustomAttribute<PlaceholderPropertyAttribute>(inherit: true);
                if (propAttribute == null)
                    continue;

                results.Add(new PlaceholderPropertyDescriptor(property, propAttribute));
            }
            return results;
        }
    }

    public class PlaceholderPropertyDescriptor
    {
        public PropertyInfo Property { get; }
        public PlaceholderPropertyAttribute PropertyAttribute { get; }

        public Type PropertyType => this.Property.PropertyType;

        public PlaceholderPropertyDescriptor(PropertyInfo property, PlaceholderPropertyAttribute attribute)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            if (attribute == null)
                throw new ArgumentException($"Property {property.DeclaringType.Name}.{property.Name} isn't decorated with {nameof(PlaceholderPropertyAttribute)}.");

            this.Property = property;
            this.PropertyAttribute = attribute;
        }

        public PlaceholderPropertyDescriptor(PropertyInfo property)
            : this(property, property.GetCustomAttribute<PlaceholderPropertyAttribute>(inherit: true)) { }
    }
}
