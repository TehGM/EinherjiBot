namespace TehGM.EinherjiBot.RandomStatus.Placeholders
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class StatusPlaceholderAttribute : Attribute, IEquatable<StatusPlaceholderAttribute>, IEquatable<string>
    {
        public const RegexOptions DefaultRegexOptions = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled;

        public string Placeholder { get; }
        public RegexOptions RegexOptions { get; }

        public Regex PlaceholderRegex { get; }

        public StatusPlaceholderAttribute(string placeholder, RegexOptions regexOptions)
        {
            if (string.IsNullOrWhiteSpace(placeholder))
                throw new ArgumentNullException(placeholder);

            this.Placeholder = placeholder;
            this.RegexOptions = regexOptions;
            this.PlaceholderRegex = new Regex(placeholder, regexOptions);
        }

        public StatusPlaceholderAttribute(string placeholder)
            : this(placeholder, DefaultRegexOptions) { }

        public override bool Equals(object obj)
        {
            if (obj is StatusPlaceholderAttribute attr)
                return this.Equals(attr);
            if (obj is string placeholder)
                return this.Equals(placeholder);
            return false;
        }
        public bool Equals(StatusPlaceholderAttribute other)
            => other is not null && string.Equals(this.Placeholder, other.Placeholder, StringComparison.OrdinalIgnoreCase);
        public bool Equals(string other)
            => string.Equals(this.Placeholder, other, StringComparison.OrdinalIgnoreCase);

        public override int GetHashCode()
            => this.Placeholder.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }
}
