using System.Collections.Generic;
using GalacticWasteManagement.Scripts;

namespace GalacticWasteManagement
{
    public interface IProjectSettings
    {
        IMigrationVersioning MigrationVersioning { get; }
        List<IScriptProvider> ScriptProviders { get; }
        IScriptParser ScriptParser { get; }
    }
}