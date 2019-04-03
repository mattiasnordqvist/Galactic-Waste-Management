using System.Linq;
using System.Threading.Tasks;
using GalacticWasteManagement.Logging;
using GalacticWasteManagement.Output;
using HonestNamespace;
using JellyDust;

namespace GalacticWasteManagement
{
    public class BrownFieldMigration : MigrationBase
    {
        public static readonly string Param_Source = "source";
        public static readonly string Param_ForceRestore = "force-restore";

        public BrownFieldMigration(IProjectSettings projectSettings, ILogger logger, IOutput output, IParameters input, IConnection connection, ITransaction transaction, string name) : base(projectSettings, logger, output, input, connection, transaction, name)
        {
            AllowCreate = true;
            AllowDrop = true;

            ForceRestore = Parameters.Optional(new InputBool(Param_ForceRestore, "force restore from backup"), false);
            Source = Parameters.Required(new InputFile(Param_Source, ".bak-file to restore from", true));
        }

        public Param<bool> ForceRestore { get; }
        public Param<string> Source { get; }

        public override async Task ManageWaste()
        {
            var cleanRequested = ForceRestore.Get();
            var hasCleaned = false;
            var isClean = Honestly.DontKnow;

            var dbExists = await DbExist();
            var dbCreated = false;

            if (!dbExists)
            {
                Logger.Log($"No '{DatabaseName}' database found. It will be created.", "warning");
                await CreateSafe();
                dbCreated = true;
                dbExists = true;
                isClean = true;
                hasCleaned = true;
                if (cleanRequested)
                {
                    Logger.Log("Parameter 'Clean' was set but ignored, since database was just created. Drop scripts are not run.", "info");
                    cleanRequested = false;
                }
            }
            Connection.DbConnection.ChangeDatabase(DatabaseName);
            if (dbCreated || cleanRequested)
            {
                await Restore(Source.Get());
            }


            var triggeringTransaction = Transaction.DbTransaction; // TODO: change this to be configurable

            // execute all scripts in Migrations in latest version.
            var scripts = GetScripts(ScriptType.Migration);
            var schema = await GetSchema(null, ScriptType.Migration);

            var migrationComparison = Compare(scripts, schema);
            if (migrationComparison.Unchanged.Count() != migrationComparison.All.Count())
            {
                Logger.Log("Older migration scripts were found. They will not be run.", "warning");
            }

            // execute all scripts in vNext.
            // if any scripts changed or deleted, drop schema and start over.
            // scripts added are just run, no matter it's usual ordering.
            var comparisonVNext = await Compare("vNext", ScriptType.vNext);
            var comparisonSeed = await Compare("local", ScriptType.Seed);

            if (comparisonVNext.Removed.Any() || comparisonVNext.Changed.Any() ||
                comparisonSeed.Removed.Any() || comparisonSeed.Changed.Any())
            {
                Logger.Log("Changed or removed scripts in vNext or Seed. Cleaning schema.", "important");
                await DropSafe();
                await Initialize();
                hasCleaned = true;
                Logger.Log($"Performing vNext migrations for '{DatabaseName}' database.", "info");
                await RunScripts(comparisonVNext.All, "vNext");
            }
            else if (comparisonVNext.New.Any())
            {
                Logger.Log("New migrations in vNext found", "info");
                Logger.Log($"Performing vNext migrations for '{DatabaseName}' database.", "info");
                await RunScripts(comparisonVNext.New, "vNext");
            }
            else
            {
                Logger.Log("No new vNext migrations", "info");
            }

            // execute changed and new scripts in RunIfChanged.
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

            if (hasCleaned && comparisonSeed.All.Any())
            {
                Logger.Log($"Running seeds for '{DatabaseName}' database.", "info");
                await RunScripts(comparisonSeed.All, "Local");
            }
            else if (comparisonSeed.New.Any())
            {
                Logger.Log("New seed scripts found", "info");
                Logger.Log($"Running seeds for '{DatabaseName}' database.", "info");
                await RunScripts(comparisonSeed.New, "Local");
            }
            else
            {
                Logger.Log("No new Seed scripts", "info");
            }
        }
    }
}
