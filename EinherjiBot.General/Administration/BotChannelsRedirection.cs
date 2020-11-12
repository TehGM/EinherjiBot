using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TehGM.EinherjiBot.Administration
{
    public class BotChannelsRedirection
    {
        public HashSet<ulong> AllowedChannelIDs { get; set; }
        public ulong[] BotIDs { get; set; }
        public string[] Patterns { get; set; }
        private Regex[] _regexes;

        public IEnumerable<Regex> GetRegexes()
        {
            if (this._regexes?.Any() != true && this.Patterns?.Any() == true)
            {
                _regexes = new Regex[this.Patterns.Length];
                for (int i = 0; i < this.Patterns.Length; i++)
                    _regexes[i] = new Regex(this.Patterns[i], RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.Compiled);
            }
            return _regexes ?? Enumerable.Empty<Regex>();
        }
    }
}
