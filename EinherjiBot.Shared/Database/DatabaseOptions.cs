namespace TehGM.EinherjiBot.Database
{
    public class DatabaseOptions
    {
        public const string MiscellaneousCollectionName = "Miscellaneous";

        public string ConnectionString { get; set; }

        // databases
        public string DatabaseName { get; set; } = "Einherji";
    }
}
