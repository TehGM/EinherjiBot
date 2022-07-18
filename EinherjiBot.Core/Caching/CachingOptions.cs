namespace TehGM.EinherjiBot.Caching
{
    public class CachingOptions
    {
        public bool Enabled { get; set; } = true;
        public TimeSpan Lifetime { get; set; } = TimeSpan.FromMinutes(30);
    }
}
