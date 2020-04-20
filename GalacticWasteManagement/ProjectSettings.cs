using System.Collections.Generic;
using GalacticWasteManagement.Scripts;

namespace GalacticWasteManagement
{
    public class ProjectSettings : IProjectSettings
    {
        public ProjectSettings(IMigrationVersioning migrationVersioning, IScriptParser scriptParser, List<IScriptProvider> scriptProviders)
        {
            MigrationVersioning = migrationVersioning;
            ScriptProviders = scriptProviders;
            ScriptParser = scriptParser;
        }
        public IMigrationVersioning MigrationVersioning { get; set; }
        public List<IScriptProvider> ScriptProviders { get; set; }

        public IScriptParser ScriptParser { get; set; }
    }
}
