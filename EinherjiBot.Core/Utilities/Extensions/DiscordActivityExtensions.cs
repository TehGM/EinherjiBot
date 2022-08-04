using Discord;

namespace TehGM.EinherjiBot
{
    public static class DiscordActivityExtensions
    {
        public static string ToDisplayString(this ActivityType activity)
        {
            switch (activity)
            {
                case ActivityType.Playing:
                    return "Playing";
                case ActivityType.Streaming:
                    return "Streaming";
                case ActivityType.Watching:
                    return "Watching";
                case ActivityType.Listening:
                    return "Listening to";
                case ActivityType.CustomStatus:
                    return string.Empty;
                default:
                    return activity.ToString();
            }
        }
    }
}
