using System.Text.RegularExpressions;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class RegexCommandAttribute : Attribute
    {
        public const RegexOptions DefaultRegexOptions = RegexOptions.CultureInvariant | RegexOptions.Multiline;

        public string Pattern { get; }
        public RegexOptions RegexOptions { get; }

        public RegexCommandAttribute(string pattern)
            : this(pattern, DefaultRegexOptions) { }

        public RegexCommandAttribute(string pattern, RegexOptions options)
        {
            this.Pattern = pattern;
            this.RegexOptions = options;
        }
    }
}
