using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalacticWasteManagement.Logging;
using GalacticWasteManagement.Output;
using GalacticWasteManagement.Scripts;
using JellyDust;
using JellyDust.Dapper;
using StackExchange.Profiling;

namespace GalacticWasteManagement
{
    public abstract class MigrationBase : IMigration
    {
        protected IProjectSettings ProjectSettings { get; }
        protected ILogger Logger { get; }
        public IOutput Output { get; }

        protected IConnection Connection { get; }
        protected ITransaction Transaction { get; }


        protected bool AllowCleanSchema { get; set; } = false;
        protected bool AllowDrop { get; set; } = false;
        protected bool AllowCreate { get; set; } = false;
        public string DatabaseName { get; set; }

        public Dictionary<string, string> ScriptVariables { get; set; }

        public MigrationBase(IProjectSettings projectSettings, ILogger logger, IOutput output, IConnection connection, ITransaction transaction)
        {
            Logger = logger;
            Output = output;
            Connection = connection;
            Transaction = transaction;
            ProjectSettings = projectSettings;
        }

        protected async Task<SchemaVersionJournalEntry> RunScripts(IEnumerable<IScript> scripts, string version, ScriptType? type, bool journal = true, string database = null)
        {
            Connection.DbConnection.ChangeDatabase(database ?? DatabaseName);
            var lastVersion = journal ? await GetLastSchemaVersionJournalEntry() : null;
            foreach (var script in scripts)
            {
                Logger.Log($"Executing script '{script.Name}'", "info");
                using (var step = Output.MiniProfiler.Step(script.Name))
                {
                    await script.ApplyAsync(Connection, ScriptVariables);
                }

                if (journal)
                {
                    var nextVersion = version ?? ProjectSettings.MigrationVersioning.DetermineVersion(script);
                    var nextSchemaJournalVersion = new SchemaVersionJournalEntry
                    {
                        Name = script.Name,
                        Type = type.ToString(),
                        Applied = DateTime.Now,
                        Version = nextVersion,
                        Hash = script.GetHash()
                    };
                    using (var step = Output.MiniProfiler.Step($"Journaling {script.Name}"))
                    {
                        await Connection.ExecuteAsync($"INSERT INTO SchemaVersionJournal (Version, [Type], Name, Applied, Hash) values (@Version, @Type, @Name, @Applied, @Hash)", nextSchemaJournalVersion);
                    }

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

        public async Task<bool> DbExist()
        {
            var currentDb = Connection.DbConnection.Database;
            Connection.DbConnection.ChangeDatabase("master");
            try
            {
                return (await Connection.ExecuteScalarAsync<int>("SELECT 1 FROM sys.databases WHERE name = @dbName", new { dbName = DatabaseName })) == 1;
            }
            finally
            {
                Connection.DbConnection.ChangeDatabase(currentDb);
            }
        }

        protected async Task CleanSchemaSafe()
        {
            if (!AllowCleanSchema)
            {
                Logger.Log($"Cleaning schema in current field is prohibited!", "warning");
            }

            Logger.Log($"Cleaning '{DatabaseName}' database schema.", "important");
            await DropSafe();
            await FirstRun();
        }

        protected Task DropSafe()
        {
            if (!AllowDrop)
            {
                Logger.Log($"Dropping schema in current field is prohibited!", "warning");
            }

            Logger.Log($"Dropping '{DatabaseName}' database schema.", "important");
            return RunScripts(GetScripts(ScriptType.Drop), null, null, false);
        }

        protected Task CreateSafe()
        {
            if (!AllowCreate)
            {
                Logger.Log($"Creating database in current field is prohibited!", "warning");
            }

            Logger.Log($"Creating database '{DatabaseName}'.", "important");
            return RunScripts(GetScripts(ScriptType.Create), null, null, false, "master");
        }

        protected Task FirstRun()
        {
            return RunScripts(GetScripts(ScriptType.FirstRun), null, null, false);
        }

        protected static SchemaComparison Compare(IEnumerable<IScript> scripts, IEnumerable<SchemaVersionJournalEntry> schema)
        {
            var all = scripts.ToList();
            var @new = scripts.Where(x => !schema.Select(s => s.Name).Contains(x.Name)).ToList();
            var changed = scripts.Join(schema, x => x.Name, x => x.Name, (x, y) => (x, y)).Where(x => x.x.GetHash() != x.y.Hash).ToList();
            var removed = schema.Where(s => !scripts.Any(n => n.Name == s.Name)).ToList();
            var unchanged = scripts.Join(schema, x => (x.Name, x.GetHash()), x => (x.Name, x.Hash), (x, y) => (x, y)).ToList();
            return new SchemaComparison { All = all, New = @new, Changed = changed, Removed = removed, Unchanged = unchanged };
        }

        protected async Task<SchemaComparison> Compare(string version, ScriptType type)
        {
            var scripts = GetScripts(type);
            var schema = await GetSchema(version, type);
            return Compare(scripts, schema);
        }

        protected IEnumerable<IScript> GetScripts(ScriptType scriptType)
        {
            return ProjectSettings.ScriptProviders.SelectMany(x => x.GetScripts(scriptType));
        }

        public abstract Task ManageWaste(bool clean);
    }
}
