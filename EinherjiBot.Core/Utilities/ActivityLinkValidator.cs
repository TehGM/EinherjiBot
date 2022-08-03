namespace TehGM.EinherjiBot.Utilities
{
    public static class ActivityLinkValidator
    {
        public static IEnumerable<string> ValidLinks { get; } = new string[]
        {
            "https://twitch.tv/",
            "https://www.twitch.tv/",
            "https://youtube.com/",
            "https://www.youtube.com/"
        };

        public static bool IsLinkValid(string linkValue)
        {
            if (string.IsNullOrEmpty(linkValue))
                return true;
            return ValidLinks.Any(l => linkValue.StartsWith(l, StringComparison.OrdinalIgnoreCase));
        }

        public static Func<string, IEnumerable<string>> ValidationDelegate => value =>
        {
            if (!IsLinkValid(value))
                return new string[] { $"Stream link must be from one of the following domains: {string.Join(", ", ValidLinks)}" };
            return Enumerable.Empty<string>();
        };
    }
}
