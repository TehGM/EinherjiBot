using System;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using Serilog;
using TehGM.EinherjiBot.DataMigration.Migrations;

namespace TehGM.EinherjiBot.DataMigration
{
    class Program
    {
        private static ILogger _log;
        static async Task Main(string[] args)
        {
            _log = StartLogging();
            Settings settings = await LoadSettingsAsync(args).ConfigureAwait(false);

            _log.Information("Loading data files");
            _log.Debug("Loading {Filename}", settings.DataFileName);
            JToken dataJson = await JsonFileUtility.LoadFromFileAsync(settings.DataFileName).ConfigureAwait(false);
            _log.Debug("Loading {Filename}", settings.IntelFileName);
            JToken intelJson = await JsonFileUtility.LoadFromFileAsync(settings.IntelFileName).ConfigureAwait(false);


            _log.Information("Establishing database connection, DB {DatabaseName}", settings.DatabaseName);
            MongoClient client = new MongoClient(settings.ConnectionString);
            ConventionPack conventionPack = new ConventionPack();
            conventionPack.Add(new MapReadOnlyPropertiesConvention());
            conventionPack.Add(new GuidAsStringRepresentationConvention());
            ConventionRegistry.Register("Conventions", conventionPack, _ => true);
            IMongoDatabase db = client.GetDatabase(settings.DatabaseName);

            // run migrations
            await (new NetflixAccountMigration(_log, db, "Miscellaneous")).RunMigrationAsync(dataJson["netflixAccount"]);

            _log.Information("Done");
            Console.ReadLine();
        }

        private static Task<Settings> LoadSettingsAsync(string[] args)
        {
            // load config file name, taking environment into account
            if (args.Length == 0)
            {
                _log.Error("Specify environment. Allowed environments: dev, prod");
                Console.ReadLine();
                Environment.Exit(1);
                return Task.FromResult<Settings>(null);
            }
            string configFilename;
            switch (args[0].ToLower())
            {
                case "dev":
                    {
                        Console.Write("Are you sure you want to process on DEVELOPMENT environment? (Y): ");
                        if (!Console.ReadLine().Equals("Y", StringComparison.OrdinalIgnoreCase))
                        {
                            _log.Information("Operation aborted");
                            Console.ReadLine();
                            Environment.Exit(3);
                            return Task.FromResult<Settings>(null);
                        }
                        _log.Information("Using development environment");
                        configFilename = "appsecrets.Development.json";
                    }
                    break;
                case "prod":
                    {
                        Console.Write("Are you sure you want to process on PRODUCTION environment? (Y): ");
                        if (!Console.ReadLine().Equals("Y", StringComparison.OrdinalIgnoreCase))
                        {
                            _log.Information("Operation aborted.");
                            Console.ReadLine();
                            Environment.Exit(3);
                            return Task.FromResult<Settings>(null);
                        }
                        _log.Information("Using production environment");
                        configFilename = "appsecrets.json";
                    }
                    break;
                default:
                    {
                        _log.Error("Invalid environment. Allowed environments: dev, prod");
                        Console.ReadLine();
                        Environment.Exit(2);
                        return Task.FromResult<Settings>(null);
                    }
            }

            _log.Debug("Loading config from {FileName}", configFilename);
            return Settings.LoadAsync(configFilename);
        }

        private static ILogger StartLogging()
        {
            Log.Logger = new LoggerConfiguration()
                        .WriteTo.Console()
                        .MinimumLevel.Debug()
                        .CreateLogger();
            // capture unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            return Log.Logger;
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Log.Logger.Error((Exception)e.ExceptionObject, "An exception was unhandled");
                Log.CloseAndFlush();
            }
            catch { }
        }
    }
}
