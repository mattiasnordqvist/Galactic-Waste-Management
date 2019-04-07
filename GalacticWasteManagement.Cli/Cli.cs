using GalacticWasteManagement.In;
using GalacticWasteManagement.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CommandLine;

namespace GalacticWasteManagement.Cli
{

    [Verb("migrate", HelpText = "Migrate database according to appsettings.json")]
    class MigrateOptions
    {
        [Option('c', "no-color", Required = false)]
        public bool NoColor { get; set; } = false;

        [Option('u', "no-user", Required = false)]
        public bool NoUser { get; set; } = false;
    }
    public class Cli
    {
        public void Launch<T>(string[] args)
        {
            Parser.Default.ParseArguments<MigrateOptions,object>(args)
               .MapResult(
                 (MigrateOptions opts) => Migrate<T>(opts).Result,
                 errs => 1);

        }

        private async Task<int> Migrate<T>(MigrateOptions opts)
        {
            try
            {
                var config = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: false)
                   .Build();
                var migrationConfig = config.GetSection("Migration");
                var mode = migrationConfig["Mode"];

                var input = new ConfigInput(migrationConfig.GetSection(mode));
                var wasteManager = GalacticWasteManager.Create<T>(migrationConfig["ConnectionString"]);
                if (opts.NoColor)
                {
                    wasteManager.Logger = new ConsoleLogger("") { NoColor = true };
                }
                wasteManager.Parameters.SetInput(input);
                await wasteManager.Update(mode);
            }
            finally
            {
                if (!opts.NoUser)
                {
                    Console.ReadLine();
                }
            }
            return 1;
        }
    }
}
