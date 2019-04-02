using System;
using System.Linq;
using System.Threading.Tasks;
using GalacticWasteManagement.Logging;
using GalacticWasteManagement.Output;
using HonestNamespace;
using JellyDust;

namespace GalacticWasteManagement
{
    public class GreenFieldMigration : MigrationBase
    {
        public GreenFieldMigration(IProjectSettings projectSettings, ILogger logger, IOutput output, Input input, IConnection connection, ITransaction transaction, string name = "GreenField") : base(projectSettings, logger, output, input, connection, transaction, name)
        {
            AllowCreate = true;
            AllowDrop = true;

            Clean = Input.Optional(new InputBool("clean", "force database to clean"), false);
        }

        public Param<bool> Clean { get; }

        public override async Task ManageWaste()
        {
            var cleanRequested = Clean.Get();
            var hasCleaned = false;
            var isClean = Honestly.DontKnow;

            var dbExists = await DbExist();
            var dbCreated = false;

            if (!dbExists)
            {
                Logger.Log($"Database '{DatabaseName}' not found. It will be created.", "important");
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

            if (cleanRequested && !dbCreated)
            {
                Logger.Log($"Cleaning database '{DatabaseName}' because parameter 'Clean' was set.", "info");
                await DropSafe();
                isClean = true;
                hasCleaned = true;
            }

            Connection.DbConnection.ChangeDatabase(DatabaseName);
            if (dbCreated || hasCleaned || !await SchemaVersionJournalExists())
            {
                Logger.Log("Creating table for schema versioning.", "info");
                await Initialize();
            }

           
            var triggeringTransaction = Transaction.DbTransaction; // TODO: change this to be configurable
        
            if (GetScripts(ScriptType.Migration).Any())
            {
                Logger.Log("Scripts found in Migration folder. No scripts should exist in Migration folder when doing Green Field development.", "warning");
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
