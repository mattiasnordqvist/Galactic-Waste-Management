using System.Reflection;
using GalacticWasteManagement.Scripts;
using GalacticWasteManagement.Scripts.ScriptProviders;

namespace GalacticWasteManagement
{
    public class DefaultProjectSettings<T> : ProjectSettings
    {
        public DefaultProjectSettings() : base(new DefaultMigrationVersioning(), new EmbeddedScriptProvider(Assembly.GetAssembly(typeof(T)), "Scripts"))
        {
        }
    }
}
