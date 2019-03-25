using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalacticWasteManagement.Logging;
using GalacticWasteManagement.Scripts;
using JellyDust;
using JellyDust.Dapper;

namespace GalacticWasteManagement
{
    public abstract class MigrationBase : IMigration
    {
        protected IProjectSettings ProjectSettings { get; }

        protected ILogger Logger { get; }

        protected IConnection Connection { get; }
        protected ITransaction Transaction { get; }


        protected bool AllowCleanSchema { get; set; } = false;
        protected bool AllowDrop { get; set; } = false;
        protected bool AllowCreate { get; set; } = false;

        public MigrationBase(IProjectSettings projectSettings, ILogger logger, IConnection connection, ITransaction transaction)
        {
            Logger = logger;
            Connection = connection;
            Transaction = transaction;
            ProjectSettings = projectSettings;

        }

        protected async Task<SchemaVersionJournalEntry> RunScripts(IEnumerable<IScript> scripts, WasteManagerConfiguration configuration, string version, ScriptType? type, bool journal = true, string database = null)
        {
            var scriptVariables = configuration.ScriptVariables ?? new Dictionary<string, string>();

            Connection.DbConnection.ChangeDatabase(database ?? configuration.DatabaseName);
            var lastVersion = journal ? await GetLastSchemaVersionJournalEntry() : null;
            foreach (var script in scripts)
            {
                Logger.Log($"Executing script '{script.Name}'", "info");

                await script.Apply(Connection, scriptVariables);
                if (journal)
                {
                    var nextVersion = version ?? ProjectSettings.MigrationVersioning.DetermineVersion(script);
                    var nextSchemaJournalVersion = new SchemaVersionJournalEntry
                    {
                        Name = script.Name,
                        Type = type.ToString(),
                        Applied = DateTime.Now,
                        Version = nextVersion,
                        Hashed = script.Hashed
                    };

                    await Connection.ExecuteAsync($"INSERT INTO SchemaVersionJournal (Version, [Type], Name, Applied, Hashed) values (@Version, @Type, @Name, @Applied, @Hashed)", nextSchemaJournalVersion);
                    lastVersion = nextSchemaJournalVersion;
                }
            }
            return lastVersion;

        }

        public async Task<List<SchemaVersionJournalEntry>> GetSchema(string schemaVersion = null, ScriptType? type = null)
        {
            return (await Connection.QueryAsync<SchemaVersionJournalEntry>($@"
                SELECT * FROM (
                    SELECT * FROM SchemaVersionJournal WHERE [Type] <> '{nameof(ScriptType.RunIfChanged)}'
                    UNION ALL
                    SELECT * FROM SchemaVersionJournal WHERE Id IN (
                        SELECT Max(Id) FROM SchemaVersionJournal WHERE [Type] = '{nameof(ScriptType.RunIfChanged)}' GROUP BY Name
                    )
                ) _
                WHERE ([Version] = @version OR @version IS NULL) AND ([Type] = @type OR @type IS NULL)", new { type = type?.ToString(), version = schemaVersion })
            ).ToList();
        }

        public async Task<SchemaVersionJournalEntry> GetLastSchemaVersionJournalEntry()
        {
            return (await Connection.QueryAsync<SchemaVersionJournalEntry>("SELECT TOP 1 * FROM SchemaVersionJournal ORDER BY Id DESC")).FirstOrDefault();
        }

        public async Task<bool> DbExist(string databaseName)
        {
            var currentDb = Connection.DbConnection.Database;
            Connection.DbConnection.ChangeDatabase("master");
            try
            {
                return (await Connection.ExecuteScalarAsync<int>("SELECT 1 FROM sys.databases WHERE name = @dbName", new { dbName = databaseName })) == 1;
            }
            finally
            {
                Connection.DbConnection.ChangeDatabase(currentDb);
            }
        }

        protected async Task CleanSchemaSafe(WasteManagerConfiguration updateConfig)
        {
            if (!AllowCleanSchema)
            {
                Logger.Log($"Cleaning schema in current field is prohibited!", "warning");
            }

            Logger.Log($"Cleaning '{updateConfig.DatabaseName}' database schema.", "important");
            await DropSafe(updateConfig);
            await FirstRun(updateConfig);
        }

        protected Task DropSafe(WasteManagerConfiguration updateConfig)
        {
            if (!AllowDrop)
            {
                Logger.Log($"Dropping schema in current field is prohibited!", "warning");
            }

            Logger.Log($"Dropping '{updateConfig.DatabaseName}' database schema.", "important");
            return RunScripts(ProjectSettings.ScriptProvider.GetScripts(ScriptType.Drop), updateConfig, null, null, false);
        }

        protected Task CreateSafe(WasteManagerConfiguration updateConfig)
        {
            if (!AllowCreate)
            {
                Logger.Log($"Creating database in current field is prohibited!", "warning");
            }

            Logger.Log($"Creating database '{updateConfig.DatabaseName}'.", "important");
            return RunScripts(ProjectSettings.ScriptProvider.GetScripts(ScriptType.Create), updateConfig, null, null, false, "master");
        }

        protected Task FirstRun(WasteManagerConfiguration updateConfig)
        {
            return RunScripts(ProjectSettings.ScriptProvider.GetScripts(ScriptType.FirstRun), updateConfig, null, null, false);
        }

        protected static SchemaComparison Compare(IEnumerable<IScript> scripts, IEnumerable<SchemaVersionJournalEntry> schema)
        {
            var all = scripts.ToList();
            var @new = scripts.Where(x => !schema.Select(s => s.Name).Contains(x.Name)).ToList();
            var changed = scripts.Join(schema, x => x.Name, x => x.Name, (x, y) => (x, y)).Where(x => x.x.Hashed != x.y.Hashed).ToList();//(s => schema.Any(n => n.Name == s.Name && n.Hashed != s.Hashed)).ToList();
            var removed = schema.Where(s => !scripts.Any(n => n.Name == s.Name)).ToList();
            var unchanged = scripts.Join(schema, x => (x.Name, x.Hashed), x => (x.Name, x.Hashed), (x, y) => (x, y)).ToList();
            return new SchemaComparison { All = all, New = @new, Changed = changed, Removed = removed, Unchanged = unchanged };
        }

        protected async Task<SchemaComparison> Compare(string version, ScriptType type)
        {
            var scripts = ProjectSettings.ScriptProvider.GetScripts(type);
            var schema = await GetSchema(version, type);
            return Compare(scripts, schema);
        }

        protected IEnumerable<IScript> GetScripts(ScriptType scriptType)
        {
            return ProjectSettings.ScriptProvider.GetScripts(scriptType);
        }

        public abstract Task ManageWasteInField(WasteManagerConfiguration configuration);
    }
}
