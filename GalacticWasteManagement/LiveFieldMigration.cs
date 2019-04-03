using System.Linq;
using System.Threading.Tasks;
using GalacticWasteManagement.Logging;
using GalacticWasteManagement.Output;
using JellyDust;

namespace GalacticWasteManagement
{
    public class LiveFieldMigration : MigrationBase
    {
        public LiveFieldMigration(IProjectSettings projectSettings, ILogger logger, IOutput output, Parameters input, IConnection connection, ITransaction transaction, string name = "LiveField") : base(projectSettings, logger, output, input, connection, transaction, name)
        {
            AllowCreate = true;
        }

        public override async Task ManageWaste()
        {
            var dbExists = await DbExist();
            if (!dbExists)
            {
                Logger.Log($"No '{DatabaseName}' database found. It will be created.", "warning");
                await CreateSafe();
                await Initialize();
            }

            Connection.DbConnection.ChangeDatabase(DatabaseName);
            var triggeringTransaction = Transaction.DbTransaction; // TODO: change this to be configurable
            
            // execute all scripts in Migrations in latest version.
            var scripts = GetScripts(ScriptType.Migration);
            var schema = await GetSchema(null, ScriptType.Migration);

            // if any changed/removed/added on earlier versions, warn
            // all added on current version and onward should be new, run them
            var lastJournalEntry = await GetLastSchemaVersionJournalEntry();
            var v = lastJournalEntry != null ? new Scripts.Version(lastJournalEntry.Version) : null;
            var olderScripts = scripts.Where(s => v != null && ProjectSettings.MigrationVersioning.Compare(ProjectSettings.MigrationVersioning.Version(s), v) <= 0);
            var newerScripts = scripts.Where(s => v == null || ProjectSettings.MigrationVersioning.Compare(ProjectSettings.MigrationVersioning.Version(s), v) > 0);
            var olderSchema = schema.Where(s => (v != null && ProjectSettings.MigrationVersioning.Compare(s.VersionStringForJournaling, v) <= 0));
            var newerSchema = schema.Where(s => (v == null || ProjectSettings.MigrationVersioning.Compare(s.VersionStringForJournaling, v) > 0));
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
                lastVersion = await RunScripts(scripts, null);
            }

            if (GetScripts(ScriptType.vNext).Any())
            {
                Logger.Log("Scripts found in vNext folder. No scripts should exist in vNext folder when doing Live Field migrations. Scripts will not be run.", "warning");
            }

            // execute changed and new scripts in RunIfChanged.
            var changedComparison = await Compare(null, ScriptType.RunIfChanged);

            if (changedComparison.New.Any() || changedComparison.Changed.Any())
            {
                Logger.Log("Found changed or added RunIfChanged-scripts.", "info");
                await RunScripts(changedComparison.Changed.Select(x => x.script), lastVersion.Version);
                await RunScripts(changedComparison.New, lastVersion.Version);
            }
            else
            {
                Logger.Log("No new or changed RunIfChanged scripts", "info");
            }
        }

    
    }
}
