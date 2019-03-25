using System;
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
        private readonly string _connectionString;
        private readonly IOutput _output;
        private readonly ILogger _logger;
        public IProjectSettings ProjectSettings { get; set; }

        public GalacticWasteManager(IProjectSettings projectSettings, string connectionString, ILogger logger, IOutput output = null)
        {
            ProjectSettings = projectSettings;
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _output = output;
        }



        public async Task Update(WasteManagerConfiguration configuration)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(_connectionString)
            {
                InitialCatalog = "master"
                // - But what if we can't connect to master?!
                // - But what if we can't connect to db in connectionstring!?
                // - But what if we just handle stuff accordingly?!
            };

            var defaultScriptProvider = new EmbeddedScriptProvider(Assembly.GetAssembly(typeof(MigrationBase)), "Scripts.Defaults");
            ProjectSettings.ScriptProvider = new CompositeScriptProvider(defaultScriptProvider, ProjectSettings.ScriptProvider);
            using (var uow = new UnitOfWork(new TransactionFactory(), new ConnectionFactory(connectionStringBuilder.ConnectionString, _output)))
            {
                _logger.Log(" #### GALACTIC WASTE MANAGER ENGAGED #### ", "unicorn");
                _logger.Log($"Managing galactic waste in {configuration.DatabaseName}", "important");
                var migration = configuration.GetMigration(ProjectSettings, _logger, uow.Connection, uow.Transaction);
                await migration.ManageWasteInField(configuration);
                uow.Commit();
                _logger.Log("Galactic waste has been managed!", "success");
                _output?.Dump();
            }
        }
    }
}
