using SuperNotUnderstandableInputHandling;
using System.Linq;
using System.Threading.Tasks;

namespace GalacticWasteManagement
{
    public class BrownFieldMigration : MigrationBase
    {
        public static readonly string Param_Source = "source";
        public static readonly string Param_Clean = "clean";

        public BrownFieldMigration()
        {
            Name = "BrownField";
            AllowDrop = true;
        }

        public Param<bool> Clean => Parameters.Optional(new InputBool(Param_Clean, "force restore from backup or clean if no source specified"), false);
        public Param<string> Source => Parameters.Optional(new InputFile(Param_Source, ".bak-file to restore from", true), null);

        /// <summary>
        /// If db does not exist, create and initialize
        /// If(
        /// * Changed or Removed Seed-scripts ||
        /// * Changed or Removed vNext-scripts ||
        /// * Removed RunIfChanged-scripts ||
        /// * Force-Restore)
        /// Then If Source is set, do restore,
        /// otherwise clean and initialize.
        /// 
        /// * Run MigrationScript like in LiveField
        /// * Run vNext-scripts like in greenfield
        /// * Run RunIfChangedScripts like in greenfield
        /// * Run Seeds like in greenfield
        /// </summary>
        /// <returns></returns>
        public override async Task ManageWaste()
        {
            var shouldCleanDatabase = await ShouldClean();
            if (shouldCleanDatabase)
            {
                if (Source.Get() == null)
                {
                    if (shouldCleanDatabase)
                    {
                        Logger.Log($"Cleaning database '{DatabaseName}'.", "info");
                        await DropSafe();
                    }

                    var shouldInitializeDatabase = shouldCleanDatabase || !await SchemaVersionJournalExists();
                    if (shouldInitializeDatabase)
                    {
                        Logger.Log("Creating table for schema versioning.", "info");
                        await Initialize();
                    }
                }
                else
                {
                    Logger.Log($"Restoring database '{DatabaseName}' from '{Source.Get()}'.", "info");
                    await Restore(Source.Get());
                }
            }

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
                lastJournalEntry = await RunScripts(newerComparison.New);
            }

            var comparisonVNext = await Compare("vNext", ScriptType.vNext);

            if (comparisonVNext.New.Any())
            {
                Logger.Log($"Performing vNext migrations for '{DatabaseName}' database.", "info");
                await RunScripts(comparisonVNext.New, "vNext");
            }
            else
            {
                Logger.Log("No new vNext migrations", "info");
            }

            var changedComparison = await Compare(null, ScriptType.RunIfChanged);
            if (changedComparison.New.Any() || changedComparison.Changed.Any())
            {
                Logger.Log("Found changed or added RunIfChanged-scripts.", "info");
                await RunScripts(changedComparison.Changed.Select(x => x.script), "vNext");
                await RunScripts(changedComparison.New, "vNext");
            }
            else
            {
                Logger.Log("No new or changed RunIfChanged migrations", "info");
            }

            var comparisonSeed = await Compare("local", ScriptType.Seed);
            if (comparisonSeed.New.Any())
            {
                Logger.Log($"Running seeds for '{DatabaseName}' database.", "info");
                await RunScripts(comparisonSeed.New, "Local");
            }
            else
            {
                Logger.Log("No new Seed scripts", "info");
            }
        }

        private async Task<bool> ShouldClean()
        {
            var schemaVersionTableExists = await SchemaVersionJournalExists();
            if (schemaVersionTableExists)
            {
                if (Clean.Get())
                {
                    return true;
                }
                var comparisonVNext = await Compare("vNext", ScriptType.vNext);
                var comparisonSeed = await Compare("local", ScriptType.Seed);
                var comparisonRunIfChanged = await Compare(null, ScriptType.RunIfChanged);

                return
                comparisonVNext.Removed.Any() || comparisonVNext.Changed.Any() ||
                comparisonSeed.Removed.Any() || comparisonSeed.Changed.Any() ||
                comparisonRunIfChanged.Removed.Any(x => x.Version == "vNext");
            }

            return false;
        }
    }
}
