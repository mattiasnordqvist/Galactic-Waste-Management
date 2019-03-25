using System;
using System.Linq;
using System.Threading.Tasks;
using GalacticWasteManagement.Logging;
using GalacticWasteManagement.Scripts;
using JellyDust;

namespace GalacticWasteManagement
{
    public class LiveField : FieldBase
    {
        public LiveField(ILogger logger, Func<string, string> getSchemaVersion) : base(logger, getSchemaVersion)
        {
            AllowCreate = true;
        }

        public override async Task ManageWasteInField(IConnection connection, ITransaction transaction, WasteManagerConfiguration configuration, IScriptProvider scriptProvider)
        {
            Connection = connection;
            ScriptProvider = scriptProvider;
            var dbExists = await DbExist(configuration.DatabaseName);
            if (!dbExists)
            {
                Logger.Log($"No '{configuration.DatabaseName}' database found. It will be created.", "warning");
                await CreateSafe(configuration);
                Logger.Log("Creating table for schema versioning.", "info");
                await FirstRun(configuration);
            }

            Connection.DbConnection.ChangeDatabase(configuration.DatabaseName);

            // execute all scripts in Migrations in latest version.
            var scripts = ScriptProvider.GetScripts(ScriptType.Migration);
            var schema = await GetSchema(null, ScriptType.Migration);
            // if any changed/removed/added on earlier versions, warn
            // all added on current version and onward should be new, run them
            var lastJournalEntry = await GetLastSchemaVersionJournalEntry();
            string v = lastJournalEntry != null ? _getSchemaVersion(lastJournalEntry.Version) : null;
            var olderScripts = scripts.Where(s => v != null && _getSchemaVersion(s.Name).CompareTo(v) <= 0);
            var newerScripts = scripts.Where(s => v == null || _getSchemaVersion(s.Name).CompareTo(v) > 0);
            var olderSchema = schema.Where(s => v != null && _getSchemaVersion(s.Name).CompareTo(v) <= 0);
            var newerSchema = schema.Where(s => v == null || _getSchemaVersion(s.Name).CompareTo(v) > 0);
            var lastVersion = lastJournalEntry;
            var olderComparison = Compare(olderScripts, olderSchema);
            if (olderComparison.Removed.Any() || olderComparison.Changed.Any() || olderComparison.New.Any())
            {
                Logger.Log("Older migration scripts were found. They will not be run.", "warning");
            }

            var newerComparison = Compare(newerScripts, newerSchema);

            if (newerComparison.New.Any())
            {
                Logger.Log("New migration scripts were found and will be run.", "info");
                lastVersion = await RunScripts(scripts, configuration, null, ScriptType.Migration);
            }

            if (ScriptProvider.GetScripts(ScriptType.vNext).Any())
            {
                Logger.Log("Scripts found in vNext folder. No scripts should exist in vNext folder when doing Live Field migrations. Scripts will not be run.", "warning");
            }

            // execute changed and new scripts in RunIfChanged.
            var changedComparison = await Compare(null, ScriptType.RunIfChanged);

            if (changedComparison.New.Any() || changedComparison.Changed.Any())
            {
                Logger.Log("Found changed or added RunIfChanged-scripts.", "info");
                await RunScripts(changedComparison.Changed.Select(x => x.script), configuration, lastVersion.Version, ScriptType.RunIfChanged);
                await RunScripts(changedComparison.New, configuration, lastVersion.Version, ScriptType.RunIfChanged);
            }
            else
            {
                Logger.Log("No new or changed RunIfChanged scripts", "info");
            }
        }
    }
}
