using System;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;
using GalacticWasteManagement.Logging;
using GalacticWasteManagement.Output;
using GalacticWasteManagement.Scripts;
using GalacticWasteManagement.Scripts.ScriptProviders;
using JellyDust;

namespace GalacticWasteManagement
{
    public class GalacticWasteManager : IGalacticWasteManager
    {
        private readonly IScriptProvider _scriptProvider;
        private readonly string _connectionString;
        private readonly IOutput _output;
        private readonly ILogger _logger;

        public GalacticWasteManager(IScriptProvider scriptProvider, string connectionString, ILogger logger, IOutput output = null)
        {
            _scriptProvider = scriptProvider ?? throw new ArgumentNullException(nameof(scriptProvider));
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _output = output;
        }



        public async Task Update(WasteManagerConfiguration configuration, IField field)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(_connectionString)
            {
                InitialCatalog = "master"
                // - But what if we can't connect to master?!
                // - But what if we can't connect to db in connectionstring!?
                // - But what if we just handle stuff accordingly?!
            };

            var defaultScriptProvider = new EmbeddedScriptProvider(Assembly.GetAssembly(typeof(FieldBase)), "Scripts.Defaults");
            var scriptProvider = new CompositeScriptProvider(defaultScriptProvider, _scriptProvider);
            using (var uow = new UnitOfWork(new TransactionFactory(), new ConnectionFactory(connectionStringBuilder.ConnectionString, _output)))
            {
                _logger.Log($"Managing galactic waste in {configuration.DatabaseName}", "important");
                await field.ManageWasteInField(uow.Connection, uow.Transaction, configuration, scriptProvider);
                _logger.Log("Galactic waste has been managed!", "success");
            }
        }
    }
}
