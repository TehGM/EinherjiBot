using MongoDB.Driver;

namespace TehGM.EinherjiBot.Database
{
    public static class MongoCollations
    {
        private static readonly Lazy<Collation> _caseInsensitiveCollation = new Lazy<Collation>(() => new Collation("en", strength: CollationStrength.Primary));
        public static Collation CaseInsensitive => _caseInsensitiveCollation.Value;
    }
}
