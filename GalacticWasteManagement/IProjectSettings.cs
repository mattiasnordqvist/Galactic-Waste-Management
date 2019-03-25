using GalacticWasteManagement.Scripts;

namespace GalacticWasteManagement
{
    public interface IProjectSettings
    {
        IMigrationVersioning MigrationVersioning { get; }
        IScriptProvider ScriptProvider { get; set; }
    }
}