using System.Collections.Generic;
using System.Reflection;
using GalacticWasteManagement.Scripts;
using GalacticWasteManagement.Scripts.ScriptProviders;

namespace GalacticWasteManagement
{
    public class DefaultProjectSettings<T> : ProjectSettings
    {
        public DefaultProjectSettings() : base(
            new DefaultMigrationVersioning(),
            new List<IScriptProvider>
            {
                new EmbeddedScriptProvider(Assembly.GetAssembly(typeof(MigrationBase)), "Scripts.Defaults"),
                new EmbeddedScriptProvider(Assembly.GetAssembly(typeof(T)), "Scripts")
            }){ }
    }
}
