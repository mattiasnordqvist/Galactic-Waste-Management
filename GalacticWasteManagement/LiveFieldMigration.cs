using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalacticWasteManagement.Logging;
using GalacticWasteManagement.Output;
using JellyDust;

namespace GalacticWasteManagement
{
    public class LiveFieldMigration : MigrationBase
    {
        public LiveFieldMigration(IProjectSettings projectSettings, ILogger logger, IOutput output, IParameters input, IConnection connection, ITransaction transaction, string name = "LiveField") : base(projectSettings, logger, output, input, connection, transaction, name)
        {
            AllowCreate = true;
        }

        /// <summary>
        /// * Warn if any vNext
        /// * Quit if any _old_ migrations are changed, new or removed.
        /// * Run new migrations 
        /// * execute changed and new scripts in RunIfChanged 
        /// * (When deleting, say an SP, you should create a migration-script for deleting the SP, and then also remove the script. Removed scripts will have their hash nulled out or something, so we know we won't compare against them again.)
        /// * That's it!
        /// </summary>
        public override async Task ManageWaste()
        {
            var shouldCreateDatabase = !await DbExist();
            if (shouldCreateDatabase)
            {
                Logger.Log($"No '{DatabaseName}' database found. It will be created.", "warning");
                await CreateSafe();
            }
            Connection.DbConnection.ChangeDatabase(DatabaseName);

            var shouldInitializeDatabase = shouldCreateDatabase || !await SchemaVersionJournalExists();
            if (shouldInitializeDatabase)
            {
                Logger.Log("Creating table for schema versioning.", "info");
                await Initialize();
            }

            if (GetScripts(ScriptType.vNext).Any())
            {
                Logger.Log("Scripts found in vNext folder. vNext scripts will not be run.", "warning");
            }

            var triggeringTransaction = Transaction.DbTransaction; // TODO: change this to be configurable

            // Get all migration scripts and schema-info
            var scripts = GetScripts(ScriptType.Migration);
            var schema = await GetSchema(null, ScriptType.Migration);

            // if any changed/removed/added on earlier versions, warn
            // all added on current version and onward should be new, run them
            var lastJournalEntry = await GetLastSchemaVersionJournalEntry();
            var v = lastJournalEntry != null ? new Scripts.Version(lastJournalEntry.Version) : null;
            var olderAndSameVersionScripts = scripts.Where(s => v != null && ProjectSettings.MigrationVersioning.Compare(ProjectSettings.MigrationVersioning.Version(s), v) <= 0);
            var olderAndSameversionSchema = schema.Where(s => v != null && ProjectSettings.MigrationVersioning.Compare(s.VersionStringForJournaling, v) <= 0);
            var olderComparison = Compare(olderAndSameVersionScripts, olderAndSameversionSchema);
            if (olderComparison.Unchanged.Count() != olderComparison.All.Count())
            {
                Logger.Log("Older migration scripts were found. That's not how it's supposed to be like!", "error");
                return;
            }
            var newerScripts = scripts.Where(s => v == null || ProjectSettings.MigrationVersioning.Compare(ProjectSettings.MigrationVersioning.Version(s), v) > 0);
            var newerSchema = schema.Where(s => (v == null || ProjectSettings.MigrationVersioning.Compare(s.VersionStringForJournaling, v) > 0));
            var newerComparison = Compare(newerScripts, newerSchema);
            if (newerComparison.New.Count() != newerComparison.All.Count())
            {
                Logger.Log("Something strange is going on...", "error");
                return;
            }
            if (newerComparison.New.Any())
            {
                Logger.Log("New migration scripts were found and will be run.", "info");
                lastJournalEntry = await RunScripts(newerComparison.New, null);
            }

           
            // execute changed and new scripts in RunIfChanged.
            var runIfChangedComparison = await Compare(null, ScriptType.RunIfChanged);

            if (runIfChangedComparison.Removed.Any())
            {
                Logger.Log("Found removed RunIfChanged-scripts. Deprecating them.", "info");
                await Deprecate(runIfChangedComparison.Removed, lastJournalEntry.Version);
            }
            if (runIfChangedComparison.New.Any() || runIfChangedComparison.Changed.Any())
            {
                Logger.Log("Found changed or added RunIfChanged-scripts.", "info");
                if (lastJournalEntry != null)
                {
                    await RunScripts(runIfChangedComparison.Changed.Select(x => x.script), lastJournalEntry.Version);
                    await RunScripts(runIfChangedComparison.New, lastJournalEntry.Version);
                }
                else
                {
                    Logger.Log("RunIfChanged-scripts not run because no version could be extracted (you have zero migration scripts?) ", "info");
                }
              
            }
            else
            {
                Logger.Log("No new or changed RunIfChanged scripts", "info");
            }
        }

       
    }
}
