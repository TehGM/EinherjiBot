using System;

namespace TehGM.EinherjiBot.EliteDangerous
{
    public class CommunityGoalsOptions
    {
        public ulong AutoNewsChannelID { get; set; }
        public TimeSpan AutoNewsInterval { get; set; } = TimeSpan.FromHours(1.5);
        public TimeSpan CacheLifetime { get; set; } = TimeSpan.FromMinutes(5);
        public string ThumbnailURL { get; set; } = "https://i.imgur.com/2lQLSiG.png";
        public TimeSpan MaxAge { get; set; } = TimeSpan.FromDays(30);

        public string InaraAppName { get; set; }
        public string InaraApiKey { get; set; }
        public bool InaraAppInDevelopment { get; set; }
        public string InaraAppVersion { get; set; }
        public string InaraURL { get; set; } = "https://inara.cz/inapi/v1/";
    }
}
