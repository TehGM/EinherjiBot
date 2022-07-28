namespace TehGM.EinherjiBot.Security.API
{
    public class RefreshTokenOptions
    {
        public TimeSpan? Lifetime { get; set; } = TimeSpan.FromDays(5);
        public int BytesLength { get; set; } = 64;
    }
}
