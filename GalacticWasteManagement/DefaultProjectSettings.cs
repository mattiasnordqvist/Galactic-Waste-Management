using System.Collections.Generic;
using System.Reflection;
using GalacticWasteManagement.Scripts;
using GalacticWasteManagement.Scripts.EmbeddedScripts;
using GalacticWasteManagement.Scripts.ScriptProviders;

namespace GalacticWasteManagement
{
    public class DefaultProjectSettings<T> : ProjectSettings
    {
        public DefaultProjectSettings() : base(
            new DefaultMigrationVersioning(),
            new List<IScriptProvider>
            {
                new BuiltInScriptsScriptProvider(),
                new EmbeddedScriptProvider(Assembly.GetAssembly(typeof(T)), "Scripts")
            }){ }
    }
}
