namespace TehGM.EinherjiBot.Security
{
    public class BotAuthorizationResult
    {
        public bool Succeeded { get; }
        public string Reason { get; }

        private BotAuthorizationResult(bool succeeded, string reason)
        {
            this.Succeeded = succeeded;
            this.Reason = reason;
        }

        public static BotAuthorizationResult Success { get; } = new BotAuthorizationResult(true, null);

        public static BotAuthorizationResult Fail(string reason)
            => new BotAuthorizationResult(false, reason);

        private static readonly BotAuthorizationResult _fail = new BotAuthorizationResult(false, null);
        public static BotAuthorizationResult Fail() => _fail;
    }
}
