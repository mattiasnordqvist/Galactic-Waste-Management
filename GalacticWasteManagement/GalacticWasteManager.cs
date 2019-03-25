using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;
using GalacticWasteManagement.Logging;
using GalacticWasteManagement.Output;
using GalacticWasteManagement.Scripts.ScriptProviders;
using JellyDust;

namespace GalacticWasteManagement
{
    public class GalacticWasteManager : IGalacticWasteManager
    {
        public IProjectSettings ProjectSettings { get; private set; }
        public SqlConnectionStringBuilder ConnectionStringBuilder { get; private set; }
        public string DatabaseName { get; private set; }

        public IOutput Output { get; set; }
        public ILogger Logger { get; set; }
        public Dictionary<string, Func<IConnection, ITransaction, IMigration>> MigratorFactories { get; private set; }

        protected GalacticWasteManager() { }



        public async Task Update(string mode, bool clean = false, Dictionary<string, string> scriptVariables = null)
        {
            var variables = scriptVariables ?? new Dictionary<string, string>();
            variables.Add("DbName", DatabaseName);
            ConnectionStringBuilder.InitialCatalog = "master";
            // - But what if we can't connect to master?!
            // - But what if we can't connect to db in connectionstring!?
            // - But what if we just handle stuff accordingly?!

            var defaultScriptProvider = new EmbeddedScriptProvider(Assembly.GetAssembly(typeof(MigrationBase)), "Scripts.Defaults");
            //this row below is cheating
            ProjectSettings.ScriptProvider = new CompositeScriptProvider(defaultScriptProvider, ProjectSettings.ScriptProvider);

            using (var uow = new UnitOfWork(new TransactionFactory(), new ConnectionFactory(ConnectionStringBuilder.ConnectionString, Output)))
            {
                Logger.Log(" #### GALACTIC WASTE MANAGER ENGAGED #### ", "unicorn");
                Logger.Log($"Managing galactic waste in {DatabaseName}", "important");
                var migrator = MigratorFactories[mode](uow.Connection, uow.Transaction);
                migrator.DatabaseName = DatabaseName;
                migrator.ScriptVariables = variables;
                await migrator.ManageWaste(clean);
                uow.Commit();
                Logger.Log("Galactic waste has been managed!", "success");
                Output.Dump();
            }
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

            gwm.MigratorFactories = new Dictionary<string, Func<IConnection, ITransaction, IMigration>> {
                    {"GreenField", (c, t) => new GreenFieldMigration(gwm.ProjectSettings, gwm.Logger, gwm.Output, c, t) },
                    {"LiveField", (c, t) => new LiveFieldMigration(gwm.ProjectSettings, gwm.Logger, gwm.Output, c, t) }
                };
            return gwm;
        }
    }
}
