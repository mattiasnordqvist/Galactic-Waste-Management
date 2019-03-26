using System.Collections.Generic;
using GalacticWasteManagement.Scripts;

namespace GalacticWasteManagement
{
    public class ProjectSettings : IProjectSettings
    {
        public ProjectSettings(IMigrationVersioning migrationVersioning, List<IScriptProvider> scriptProviders)
        {
            MigrationVersioning = migrationVersioning;
            ScriptProviders = scriptProviders;
        }
        public IMigrationVersioning MigrationVersioning { get; set; }
        public List<IScriptProvider> ScriptProviders { get; set; }
    }
}
