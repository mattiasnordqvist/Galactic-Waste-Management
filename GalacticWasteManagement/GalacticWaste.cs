using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GalacticWasteManagement.Logging;
using GalacticWasteManagement.Scripts;
using GalacticWasteManagement.Scripts.ScriptProviders;
using GalacticWasteManagement.Utilities;
using JellyDust;
using JellyDust.Dapper;

namespace GalacticWasteManagement
{
    public abstract class Galaxy
    {
        private readonly IConnection _connection;
        private readonly ILogger _logger;
        private readonly Func<IScript, string> _getSchemaVersion;
        private readonly IScriptProvider _scriptProvider;

        public bool AllowCleanSchema { get; private set; } = false;
        public bool AllowDrop { get; private set; }
        public bool AllowCreate { get; private set; }

        public Galaxy(IConnection connection, ILogger logger, Func<IScript, string> getSchemaVersion, IScriptProvider scriptProvider)
        {
            var defaultScriptProvider = new EmbeddedScriptProvider(Assembly.GetAssembly(typeof(Galaxy)), "Scripts");
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _logger = logger;
            _getSchemaVersion = getSchemaVersion;
            _scriptProvider = new CompositeScriptProvider(defaultScriptProvider, scriptProvider);
        }

        private async Task<SchemaVersionJournalEntry> RunScripts(IEnumerable<IScript> scripts, Dictionary<string, string> scriptVariables, string database, string version, ScriptType? type, bool journal = true)
        {
            scriptVariables = scriptVariables ?? new Dictionary<string, string>();

            _connection.DbConnection.ChangeDatabase(database);
            var lastVersion = await GetLastSchemaVersionJournalEntry();
            foreach (var script in scripts)
            {
                _logger.Log($"Executing script '{script.Name}'", "info");

                await script.Apply(_connection, scriptVariables);
                if (journal)
                {
                    var nextVersion = _getSchemaVersion(script);
                    var nextSchemaJournalVersion = new SchemaVersionJournalEntry
                    {
                        Name = script.Name,
                        Type = type.ToString(),
                        Applied = DateTime.Now,
                        Version = nextVersion,
                        Hashed = script.Hashed
                    };

                    await _connection.ExecuteAsync($"INSERT INTO SchemaVersionJournal (Version, [Type], ScriptName, Applied, Hashed) values (@Version, @Type, @ScriptName, @Applied, @Hashed)", new
                    {
                        nextSchemaJournalVersion
                    });
                    lastVersion = nextSchemaJournalVersion;
                }
            }
            return lastVersion;

        }

        public Task<List<SchemaVersionJournalEntry>> GetSchema(IConnection connection, string schemaVersion = null, ScriptType? type = null)
        {
            return connection.QueryAsync<SchemaVersionJournalEntry>($@"
                SELECT * FROM (
                    SELECT * FROM SchemaVersionJournal WHERE [Type] <> '{nameof(ScriptType.RunIfChanged)}'
                    UNION ALL
                    SELECT * FROM SchemaVersionJournal WHERE Id IN (
                        SELECT Max(Id) FROM SchemaVersionJournal WHERE [Type] = '{nameof(ScriptType.RunIfChanged)}' GROUP BY Name
                    )
                ) _
                WHERE ([Version] = @version OR @version IS NULL) && ([Type] == @type OR @type IS NULL)", new { type, version = schemaVersion })
            .Then(x => Task.FromResult(x.ToList()));
        }

        public Task<SchemaVersionJournalEntry> GetLastSchemaVersionJournalEntry()
        {
            return _connection.QueryAsync<SchemaVersionJournalEntry>("SELECT TOP 1 * FROM SchemaVersionJournal ORDER BY Id")
                .Then(x => Task.FromResult(x.FirstOrDefault()));
        }

        public Task<bool> DbExist(string databaseName)
        {
            var currentDb = _connection.DbConnection.Database;
            _connection.DbConnection.ChangeDatabase("master");
            try
            {
                return _connection.ExecuteScalarAsync<int>("SELECT 1 FROM sys.databases WHERE name = @dbName", new { dbName = databaseName })
                    .Then(x => Task.FromResult(x == 1));
            }
            finally
            {
                _connection.DbConnection.ChangeDatabase(currentDb);
            }
        }

        protected async Task CleanSchemaSafe(UpdateDatabaseConfig updateConfig)
        {
            if (!AllowCleanSchema)
            {
                _logger.Log($"Cleaning schema in current galaxy is prohibited!", "warning");
            }
            _logger.Log($"Cleaning '{updateConfig.DatabaseName}' database schema.", "important");
            await DropSafe(updateConfig);
            await FirstRun(updateConfig);
        }

        public Task DropSafe(UpdateDatabaseConfig updateConfig)
        {
            if (!AllowDrop)
            {
                _logger.Log($"Dropping schema in current galaxy is prohibited!", "warning");
            }

            _logger.Log($"Dropping '{updateConfig.DatabaseName}' database schema.", "important");
            return RunScripts(_scriptProvider.GetScripts(ScriptType.Drop), updateConfig.ScriptVariables, updateConfig.DatabaseName, null, null, false);
        }

        public Task CreateSafe(UpdateDatabaseConfig updateConfig)
        {
            if (!AllowCreate)
            {
                _logger.Log($"Creating database in current galaxy is prohibited!", "warning");
            }

            _logger.Log($"Create '{updateConfig.DatabaseName}' database.", "important");
            return RunScripts(_scriptProvider.GetScripts(ScriptType.Create), updateConfig.ScriptVariables, "master", null, null, false);
        }

        public Task FirstRun(UpdateDatabaseConfig updateConfig)
        {
            return RunScripts(_scriptProvider.GetScripts(ScriptType.FirstRun), updateConfig.ScriptVariables, updateConfig.DatabaseName, null, null, false);
        }
    }
}
