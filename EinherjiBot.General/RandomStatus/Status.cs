using Discord;

namespace TehGM.EinherjiBot.RandomStatus
{
    public class Status
    {
        public string Text { get; set; } = null;
        public string Link { get; set; } = null;
        public bool IsAdvanced { get; set; } = false;
        public ActivityType ActivityType { get; set; } = ActivityType.CustomStatus;

        public override string ToString()
        {
            switch (this.ActivityType)
            {
                case ActivityType.Playing:
                    return $"Playing {this.Text}";
                case ActivityType.Streaming:
                    return $"Streaming {this.Text}";
                case ActivityType.Watching:
                    return $"Watching {this.Text}";
                case ActivityType.Listening:
                    return $"Listening to {this.Text}";
                case ActivityType.CustomStatus:
                    return this.Text;
                default:
                    return $"{this.ActivityType} {this.Text}";
            }
        }
    }
}
