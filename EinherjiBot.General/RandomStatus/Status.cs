using DSharpPlus.Entities;

namespace TehGM.EinherjiBot.RandomStatus
{
    public class Status
    {
        public string Text { get; set; } = null;
        public string Link { get; set; } = null;
        public ActivityType ActivityType { get; set; } = ActivityType.Custom;

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
                case ActivityType.ListeningTo:
                    return $"Listening to {this.Text}";
                case ActivityType.Custom:
                    return this.Text;
                default:
                    return $"{this.ActivityType} {this.Text}";
            }
        }
    }
}
