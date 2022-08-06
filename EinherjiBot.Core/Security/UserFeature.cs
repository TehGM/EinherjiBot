namespace TehGM.EinherjiBot.Security
{
    /// <summary>Tells client which features to display to user.</summary>
    public static class UserFeature
    {
        /// <summary>Indicates that user has access to any game server, or is able to create one.</summary>
        public const string GameServers = "GAMESERVERS";
        /// <summary>Indicates that user has access to any shared account, or is able to create one.</summary>
        public const string SharedAccounts = "SHAREDACCOUNTS";
        /// <summary>Indicates that user has access to intel panel.</summary>
        public const string Intel = "INTEL";
    }
}
