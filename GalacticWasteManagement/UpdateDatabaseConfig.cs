using System.Collections.Generic;

namespace GalacticWasteManagement
{
    public class WasteManagerConfiguration
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
        public bool Clean { get; set; }

        public bool CreateDatabaseIfNotExist { get; set; }
    }
}
