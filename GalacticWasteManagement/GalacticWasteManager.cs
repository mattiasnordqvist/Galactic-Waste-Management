using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using GalacticWasteManagement.Logging;
using GalacticWasteManagement.Output;
using JellyDust;

namespace GalacticWasteManagement
{
    // Galactic Waste Manager almost looks like IoC-container, doesn't it? Maybe it can be replaced.
    public class GalacticWasteManager : IGalacticWasteManager
    {
        public IProjectSettings ProjectSettings { get; private set; }
        public SqlConnectionStringBuilder ConnectionStringBuilder { get; private set; }
        public string DatabaseName { get; private set; }
        public IOutput Output { get; set; }
        public Input Input { get; set; }
        public ILogger Logger { get; set; }
        public static Dictionary<string, Func<GalacticWasteManager, IConnection, ITransaction, IMigration>> MigratorFactories { get; private set; }

        static GalacticWasteManager()
        {
            var greenfield = new GreenFieldMigrationFactory();
            var liveField = new LiveFieldMigrationFactory();
            var brownField = new BrownFieldMigrationFactory();
            MigratorFactories = new Dictionary<string, Func<GalacticWasteManager, IConnection, ITransaction, IMigration>> {
                { greenfield.Name, greenfield.Create },
                { liveField.Name, liveField.Create },
                { brownField.Name, brownField.Create }
            };
        }

        protected GalacticWasteManager() { }


        public async Task Update(Func<GalacticWasteManager, IConnection, ITransaction, IMigration> migratorFactory, bool clean = false, Dictionary<string, string> scriptVariables = null)
        {
            try
            {
                var variables = scriptVariables ?? new Dictionary<string, string>();
                variables.Add("DbName", DatabaseName);
                ConnectionStringBuilder.InitialCatalog = "master";
                // - But what if we can't connect to master?!
                // - But what if we can't connect to db in connectionstring!?
                // - But what if we just handle stuff accordingly?!

                using (var uow = new UnitOfWork(new TransactionFactory(), new ConnectionFactory(ConnectionStringBuilder.ConnectionString, Output)))
                {
                    Logger.Log(" #### GALACTIC WASTE MANAGER ENGAGED #### ", "unicorn");
                    Logger.Log($"Managing galactic waste in {DatabaseName}", "important");
                    var migrator = migratorFactory(this, uow.Connection, uow.Transaction);
                    migrator.DatabaseName = DatabaseName;
                    migrator.ScriptVariables = variables;
                    Logger.Log($"Running {migrator.Name} mode", "important");
                    await migrator.ManageWaste();
                    uow.Commit();
                    Logger.Log("Galactic waste has been managed!", "success");
                    Output.Dump();
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, "error");
                throw;
            }
        }
        public async Task Update(string mode, bool clean = false, Dictionary<string, string> scriptVariables = null)
        {
            await Update(MigratorFactories[mode], clean, scriptVariables);
        }

        public static GalacticWasteManager Create<T>(string connectionString, Input input)
        {
            return Create(new DefaultProjectSettings<T>(), connectionString, input);
        }

        public static GalacticWasteManager Create(IProjectSettings projectSettings, string connectionString, Input input)
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
                Input = input
            };

            return gwm;
        }
    }
}
