﻿namespace TehGM.EinherjiBot.Database
{
    public class DatabaseOptions
    {
        public string ConnectionString { get; set; }

        // databases
        public string DatabaseName { get; set; } = "Einherji";

        // collections
        public const string MiscellaneousCollectionName = "Miscellaneous";
        public string UserDataCollectionName { get; set; } = "UserData";
        public string StellarisModsCollectionName { get; set; } = "StellarisMods";
        public string PatchbotGamesCollectionName { get; set; } = "PatchbotGames";
        public string EliteCommunityGoalsCollectionName { get; set; } = "EliteCommunityGoals";
    }
}
