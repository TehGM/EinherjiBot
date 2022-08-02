namespace TehGM.EinherjiBot.Security.Authorization
{
    public class DiscordAuthorizationResult
    {
        public bool Succeeded { get; }
        public string Reason { get; }

        private DiscordAuthorizationResult(bool succeeded, string reason)
        {
            this.Succeeded = succeeded;
            this.Reason = reason;
        }

        public static DiscordAuthorizationResult Success { get; } = new DiscordAuthorizationResult(true, null);

        public static DiscordAuthorizationResult Fail(string reason)
            => new DiscordAuthorizationResult(false, reason);

        private static readonly DiscordAuthorizationResult _fail = new DiscordAuthorizationResult(false, null);
        public static DiscordAuthorizationResult Fail() => _fail;
    }
}
