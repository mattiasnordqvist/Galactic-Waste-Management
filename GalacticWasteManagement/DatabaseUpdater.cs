using System;
using System.Data.SqlClient;
using GalacticWasteManagement.Logging;
using GalacticWasteManagement.Output;
using GalacticWasteManagement.Scripts;
using JellyDust;

namespace GalacticWasteManagement
{
    public class DatabaseUpdater : IDatabaseUpdater
    {
        private readonly IScriptProvider _scriptProvider;
        private readonly string _connectionString;
        private readonly IOutput _output;
        private readonly ILogger _logger;

        public DatabaseUpdater(IScriptProvider scriptProvider, string connectionString, ILogger logger, IOutput output = null)
        {
            _scriptProvider = scriptProvider ?? throw new ArgumentNullException(nameof(scriptProvider));
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _output = output;
        }

        private static IConnection OpenConnection(string connectionString, IOutput output = null)
        {
            
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master"
                // - But what if we can't connect to master?!
                // - But what if we can't connect to db in connectionstring!?
                // - But what if we just handle stuff accordingly?!
            };

            return new Connection(new ConnectionFactory(connectionStringBuilder.ConnectionString, output));
        }

        public void Update(UpdateDatabaseConfig updateDatabaseConfig)
        {
            _logger.Log($"Managing galactic waste in {updateDatabaseConfig.DatabaseName}", "important");
        }
    }
}
