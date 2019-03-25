using System.Collections.Generic;
using GalacticWasteManagement.Logging;
using JellyDust;

namespace GalacticWasteManagement
{
    public abstract class WasteManagerConfiguration
    {
        private string _databaseName;

        public string DatabaseName
        {
            get => _databaseName;
            set
            {
                ScriptVariables["DbName"] = value;
                _databaseName = value;
            }
        }
        public Dictionary<string, string> ScriptVariables { get; set; } = new Dictionary<string, string>();
        public bool Clean { get; set; } = false;

        public abstract IMigration GetMigration(IProjectSettings projectSettings, ILogger logger, IConnection connection, ITransaction transaction);
    }
}
