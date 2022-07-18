using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using TehGM.EinherjiBot.Database.Conventions;

namespace TehGM.EinherjiBot.Database.Services
{
    public class MongoConnection : IMongoConnection
    {
        public MongoClient Client { get; private set; }
        public event Action<MongoClient> ClientChanged;

        private readonly IOptionsMonitor<DatabaseOptions> _databaseOptions;
        private readonly ILogger<MongoConnection> _log;

        public MongoConnection(IOptionsMonitor<DatabaseOptions> databaseOptions, ILogger<MongoConnection> logger)
        {
            this._log = logger;
            this._databaseOptions = databaseOptions;

            FixMongoMapping();

            _databaseOptions.OnChange(_ =>
            {
                InitializeConnection();
                this.ClientChanged?.Invoke(this.Client);
            });
            InitializeConnection();
        }

        private void InitializeConnection()
        {
            _log.LogTrace("Establishing connection to MongoDB...");
            this.Client = new MongoClient(_databaseOptions.CurrentValue.ConnectionString);
        }

        public static void FixMongoMapping()
        {
            // because ImmutableTypeClassMapConvention messes up when there's an object that has only readonly props
            // we need to remove it. To do it, we need to... unregister default conventions and re-register them manually... sigh
            // ref: https://www.codewrecks.com/post/nosql/replace-immutable-serializer-in-mongodb/
            // src of default conventions: https://github.com/mongodb/mongo-csharp-driver/blob/master/src/MongoDB.Bson/Serialization/Conventions/DefaultConventionPack.cs
            const string packName = "__defaults__";
            ConventionRegistry.Remove(packName);

            // init mongodb mapping conventions
            ConventionPack conventions = new ConventionPack();
            conventions.Add(new ReadWriteMemberFinderConvention());
            conventions.Add(new NamedIdMemberConvention(new[] { "Id", "id", "_id", "ID" }));    // adding "ID" as a bonus here
            conventions.Add(new NamedExtraElementsMemberConvention(new[] { "ExtraElements" }));
            conventions.Add(new IgnoreExtraElementsConvention(true));   // bonus - don't throw if not all properties match
            conventions.Add(new NamedParameterCreatorMapConvention());
            conventions.Add(new StringObjectIdIdGeneratorConvention());
            conventions.Add(new LookupIdGeneratorConvention());
            // custom conventions
            conventions.Add(new MapReadOnlyPropertiesConvention());
            conventions.Add(new GuidAsStringRepresentationConvention());
            conventions.Add(new EnumRepresentationConvention(BsonType.String));
            ConventionRegistry.Register(packName, conventions, _ => true);

            // guid serialization
#pragma warning disable CS0618 // Type or member is obsolete
            BsonDefaults.GuidRepresentationMode = GuidRepresentationMode.V3;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public void RegisterClassMap<T>()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
                BsonClassMap.RegisterClassMap<T>();
        }
    }
}
