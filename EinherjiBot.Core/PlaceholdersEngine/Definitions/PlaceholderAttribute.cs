using System.Text.RegularExpressions;

namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class PlaceholderAttribute : Attribute
    {
        public string Identifier { get; }
        public PlaceholderUsage AllowedContext { get; }

        public Regex MatchingRegex { get; }

        private const string _regexParamsCharset = $"A-Za-z0-9 {PlaceholderSymbol.ParameterSplitter}{PlaceholderSymbol.KeyValueSplitter}";

        public PlaceholderAttribute(string identifier, PlaceholderUsage allowedContext)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentNullException(nameof(identifier));

            this.Identifier = identifier;
            this.AllowedContext = allowedContext;

            this.MatchingRegex = new Regex($"{PlaceholderSymbol.OpenTag}{this.Identifier}(?:{Regex.Escape(PlaceholderSymbol.ParameterSplitter)}[{Regex.Escape(_regexParamsCharset)}]*?)?{PlaceholderSymbol.CloseTag}",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline,
                TimeSpan.FromMilliseconds(250));
        }
    }
}
