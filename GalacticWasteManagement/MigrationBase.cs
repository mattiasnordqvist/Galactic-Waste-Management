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
        public IParameters Parameters { get; }
        protected IConnection Connection { get; }
        protected ITransaction Transaction { get; }
        protected bool AllowDrop { get; set; } = false;
        protected bool AllowCreate { get; set; } = false;
        public string DatabaseName { get; set; }

        public Dictionary<string, string> ScriptVariables { get; set; }
        public string Name { get; }

        public MigrationBase(IProjectSettings projectSettings, ILogger logger, IOutput output, IParameters parameters, IConnection connection, ITransaction transaction, string name)
        {
            Logger = logger;
            Output = output;
            Parameters = parameters;
            Connection = connection;
            Transaction = transaction;
            ProjectSettings = projectSettings;
            Name = name;
        }
        protected async Task Deprecate(List<SchemaVersionJournalEntry> deprecated, string version)
        {
            foreach (var script in deprecated)
            {
                var entry = new SchemaVersionJournalEntry
                {
                    Name = script.Name,
                    Type = "Deprecated",
                    Applied = DateTime.Now,
                    Version = version,
                    Hash = script.Hash
                };
                using (var step = Output.MiniProfiler.Step($"Journaling {script.Name}"))
                {
                    await Connection.ExecuteAsync("INSERT INTO SchemaVersionJournal (Version, [Type], Name, Applied, Hash) values (@Version, @Type, @Name, @Applied, @Hash)", entry);
                }
            }
        }
        protected async Task<SchemaVersionJournalEntry> RunScripts(IEnumerable<IScript> scripts, string version, string database = null)
        {
            Connection.DbConnection.ChangeDatabase(database ?? DatabaseName);
            var lastVersion = await GetLastSchemaVersionJournalEntry();
            var orderedScripts = scripts.OrderBy(x => x.Type.IsJournaled);
            if (version == null)
            {
                orderedScripts = orderedScripts.ThenBy(x => x.Type.IsJournaled ? ProjectSettings.MigrationVersioning.Version(x) : null, ProjectSettings.MigrationVersioning.VersionComparer);
            }
            orderedScripts = orderedScripts.ThenBy(x => x.Name);
            foreach (var script in orderedScripts)
            {
                Logger.Log($"Executing script '{script.Name}'", "info");
                using (var step = Output.MiniProfiler.Step(script.Name))
                {
                    await script.ApplyAsync(Connection, ScriptVariables);
                }

                if (script.Type.IsJournaled)
                {
                    var nextVersion = version ?? ProjectSettings.MigrationVersioning.Version(script).Value;
                    var nextSchemaJournalVersion = new SchemaVersionJournalEntry
                    {
                        Name = script.Name,
                        Type = script.Type.Name,
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

        private class DbFilesInfo
        {
            public string LogicalName { get; set; }
            public string PhysicalName { get; set; }
            public string TypeOfFile { get; set; }

        }

        public async Task Restore(string sourceBak, string mdfLogicalName = null, string mdfPhysicalName = null, string ldfLogicalName = null, string ldfPhysicalName = null)
        {
            var currentDb = Connection.DbConnection.Database;
            Connection.DbConnection.ChangeDatabase("master");
            try
            {
                var dbNames = await Connection.QueryAsync<DbFilesInfo>($@"
SELECT f.name LogicalName,
f.physical_name AS PhysicalName,
f.type_desc TypeOfFile
FROM sys.master_files f
INNER JOIN sys.databases d ON d.database_id = f.database_id
WHERE d.Name = '{currentDb}'");

                var rows = dbNames.Single(x => x.TypeOfFile == "ROWS");
                var log = dbNames.Single(x => x.TypeOfFile == "LOG");
                
                await Connection.ExecuteAsync($@"
RESTORE DATABASE [{currentDb}]
FROM DISK = '{sourceBak}'
WITH REPLACE
--,
--WITH MOVE '{mdfLogicalName ?? rows.LogicalName}' TO '{mdfPhysicalName ?? rows.PhysicalName}',
--MOVE '{ldfLogicalName ?? log.LogicalName}' TO '{ldfPhysicalName ?? log.PhysicalName}'");
            }
            finally
            {
                Connection.DbConnection.ChangeDatabase(currentDb);
            }

        }

        public async Task<List<SchemaVersionJournalEntry>> GetSchema(string schemaVersion = null, IScriptType type = null)
        {
            return (await Connection.QueryAsync<SchemaVersionJournalEntry>($@"
                SELECT * FROM (
                    SELECT * FROM SchemaVersionJournal WHERE [Type] <> '{ScriptType.RunIfChanged.Name}' AND [Type] <> '{ScriptType.Deprecated.Name}'
                    UNION ALL
                    SELECT a.* FROM SchemaVersionJournal a 
                    LEFT JOIN SchemaVersionJournal b on b.Name = a.Name AND b.Hash = a.Hash AND b.Id > a.Id AND b.Type = '{ScriptType.Deprecated.Name}'
                    WHERE  b.Id IS NULL AND a.Id IN (
                        SELECT Max(Id) FROM SchemaVersionJournal WHERE [Type] = '{ScriptType.RunIfChanged.Name}' GROUP BY Name
                    )
                ) _
                WHERE ([Version] = @version OR @version IS NULL) AND ([Type] = @type OR @type IS NULL)", new { type = type?.Name, version = schemaVersion })
            ).ToList();
        }

        public async Task<SchemaVersionJournalEntry> GetLastSchemaVersionJournalEntry() => (await Connection.QueryFirstOrDefaultAsync<SchemaVersionJournalEntry>(@"
IF OBJECT_ID(N'dbo.SchemaVersionJournal', N'U') IS NOT NULL
SELECT TOP 1 * FROM SchemaVersionJournal ORDER BY Id DESC
ELSE 
SELECT NULL
"));

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
        public async Task<bool> SchemaVersionJournalExists()
        {
            return (await Connection.QueryFirstOrDefaultAsync<int?>("SELECT OBJECT_ID(N'dbo.SchemaVersionJournal', N'U')")).HasValue;
        }


        protected Task DropSafe()
        {
            if (!AllowDrop)
            {
                Logger.Log($"Dropping schema in current field is prohibited!", "warning");
            }

            Logger.Log($"Dropping '{DatabaseName}' database schema.", "important");
            return RunScripts(GetScripts(ScriptType.Drop), null);
        }

        protected Task CreateSafe()
        {
            if (!AllowCreate)
            {
                Logger.Log($"Creating database in current field is prohibited!", "warning");
            }

            Logger.Log($"Creating database '{DatabaseName}'.", "important");
            return RunScripts(GetScripts(ScriptType.Create), null, "master");
        }

        protected Task Initialize()
        {
            return RunScripts(GetScripts(ScriptType.Initialize), null);
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
            return ProjectSettings.ScriptProviders.SelectMany(x => x.GetScripts(scriptType)).OrderBy(x => x.Name);
        }

        public abstract Task ManageWaste();
    }
}
