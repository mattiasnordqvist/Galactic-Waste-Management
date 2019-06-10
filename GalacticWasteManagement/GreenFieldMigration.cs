using SuperNotUnderstandableInputHandling;
using System.Linq;
using System.Threading.Tasks;

namespace GalacticWasteManagement
{
    public class GreenFieldMigration : MigrationBase
    {
        public GreenFieldMigration()
        {
            AllowDrop = true;
            Name = "GreenField";
        }

        public Param<bool> Clean => Parameters.Optional(new InputBool("clean", "force database to clean"), false);

        /// <summary>
        /// criterias for cleaning database (any):
        /// * Schema-version-table does exist &&
        /// * (Clean-parameter was set ||
        /// * Changed or Removed Seed-scripts ||
        /// * Changed or Removed vNext-scripts ||
        /// * Removed RunIfChanged-scripts)
        /// criterias for initalizing database (any)
        /// * Schema-version-table does not exist
        /// * criteria for creating database are true
        /// * critiera for cleaning database are true
        /// 
        /// run new vNext-scripts
        /// run new or changed runIfChanged-scripts
        /// run new Seed-scripts
        /// </summary>
        public override async Task ManageWaste()
        {
            if (GetScripts(ScriptType.Migration).Any())
            {
                Logger.Log("Scripts found in Migration folder. No scripts should exist in Migration folder when doing Green Field development.", "warning");
            }

            var shouldCleanDatabase = await ShouldClean();
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

            var changedComparison = await Compare("vNext", ScriptType.RunIfChanged);
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
                var comparisonRunIfChanged = await Compare("vNext", ScriptType.RunIfChanged);

                return
                comparisonVNext.Removed.Any() || comparisonVNext.Changed.Any() ||
                comparisonSeed.Removed.Any() || comparisonSeed.Changed.Any() ||
                comparisonRunIfChanged.Removed.Any();
            }

            return false;
        }
    }
}
