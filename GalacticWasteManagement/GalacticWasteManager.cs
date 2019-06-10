using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using GalacticWasteManagement.Logging;
using GalacticWasteManagement.Output;
using GalacticWasteManagement.SqlServer;
using HonestNamespace;
using JellyDust;
using JellyDust.Dapper;
using SuperNotUnderstandableInputHandling;

namespace GalacticWasteManagement
{
    // Galactic Waste Manager almost looks like IoC-container, doesn't it? Maybe it can be replaced.
    public class GalacticWasteManager
    {
        public IProjectSettings ProjectSettings { get; private set; }
        public SqlConnectionStringBuilder ConnectionStringBuilder { get; private set; }
        public string DatabaseName { get; private set; }
        public IOutput Output { get; set; }
        public IParameters Parameters { get; set; }
        public ILogger Logger { get; set; }
        public static Dictionary<string, Func<GalacticWasteManager, IMigration>> MigratorFactories { get; private set; }

        static GalacticWasteManager()
        {
            MigratorFactories = new Dictionary<string, Func<GalacticWasteManager, IMigration>>();
            AddMigratorSingleton(new GreenFieldMigration());
            AddMigratorSingleton(new LiveFieldMigration());
            AddMigratorSingleton(new BrownFieldMigration());
        }

        static void AddMigratorSingleton(IMigration migration)
        {
            MigratorFactories[migration.Name] = x => { migration.GalacticWasteManager = x; return migration; };
        }

        protected GalacticWasteManager() { }


        public async Task Update(Func<GalacticWasteManager, IMigration> migratorFactory, Dictionary<string, object> parameters = null, Dictionary<string, string> scriptVariables = null)
        {
            try
            {
                var variables = scriptVariables ?? new Dictionary<string, string>();
                variables.Add("DbName", DatabaseName);

                Logger.Log(" #### GALACTIC WASTE MANAGER ENGAGED #### ", "unicorn");
                Logger.Log($"Managing galactic waste in {DatabaseName}", "important");

                var migrator = migratorFactory(this);
                migrator.Parameters.Supply(parameters);
                migrator.DatabaseName = DatabaseName;
                migrator.ScriptVariables = variables;
                Logger.Log($"Running {migrator.Name} mode", "important");
                await MaybeCreateDatabase();
                await migrator.ManageGalacticWaste();
                Logger.Log("Galactic waste has been managed!", "success");
                Output.Dump();
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, "error");
                throw;
            }
        }

        private async Task MaybeCreateDatabase()
        {
            var masterConnection = new Connection(new ConnectionFactory(ConnectionStringBuilder.For("master").ConnectionString, Output));
            try
            {
                var dbExists = Honestly.DontKnow;
                if (!Parameters.Optional(new InputBool("skip-create", "will not create database if set"), false).Get())
                {
                    dbExists = (await masterConnection.ExecuteScalarAsync<int>("SELECT 1 FROM sys.databases WHERE name = @dbName", new { dbName = DatabaseName })) == 1;
                    if (!dbExists)
                    {
                        Logger.Log($"Database '{DatabaseName}' not found. It will be created.", "important");
                        Logger.Log($"Creating database '{DatabaseName}'.", "important");
                        await masterConnection.ExecuteAsync($@"IF(DB_ID(N'{DatabaseName}') IS NULL) BEGIN CREATE DATABASE [{DatabaseName}] END");
                        dbExists = true;
                    }
                }
                if (!dbExists.IsKnown && Parameters.Optional(new InputBool("ensure-db-exists", "halts execution if db does not exist, requires access to master db"), false).Get())
                {
                    dbExists = (await masterConnection.ExecuteScalarAsync<int>("SELECT 1 FROM sys.databases WHERE name = @dbName", new { dbName = DatabaseName })) == 1;
                    if (!dbExists)
                    {
                        throw new Exception($"Database {DatabaseName} does not exist");
                    }
                }
            }
            finally
            {
                masterConnection.Dispose();
            }
        }

        public async Task Update(string mode, Dictionary<string, object> parameters = null, Dictionary<string, string> scriptVariables = null)
        {
            await Update(MigratorFactories[mode], parameters, scriptVariables);
        }

        public static GalacticWasteManager Create<T>(string connectionString)
        {
            return Create(new DefaultProjectSettings<T>(), connectionString);
        }

        public static GalacticWasteManager Create(IProjectSettings projectSettings, string connectionString)
        {
            if (projectSettings == null)
            {
                throw new ArgumentNullException(nameof(projectSettings));
            }

            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            var gwm = new GalacticWasteManager
            {
                ProjectSettings = projectSettings,
                ConnectionStringBuilder = connectionStringBuilder,
                DatabaseName = connectionStringBuilder.InitialCatalog,
                Logger = new ConsoleLogger(connectionStringBuilder.InitialCatalog),
                Output = new NullOutput(),
            };
            gwm.Parameters = new LoggingParameters(new Parameters(new ConsoleInput(true)), gwm.Logger);

            return gwm;
        }
    }
}
