using GalacticWasteManagement.Scripts;

namespace GalacticWasteManagement
{
    public class ProjectSettings : IProjectSettings
    {
        public ProjectSettings(IMigrationVersioning migrationVersioning, IScriptProvider scriptProvider)
        {
            MigrationVersioning = migrationVersioning;
            ScriptProvider = scriptProvider;
        }
        public IMigrationVersioning MigrationVersioning { get; set; }
        public IScriptProvider ScriptProvider { get; set; }
    }
}
