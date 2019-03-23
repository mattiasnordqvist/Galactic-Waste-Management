using System.Reflection;

namespace GalacticWasteManagement.Scripts.ScriptProviders
{
    public class DefaultScriptsProvider : EmbeddedScriptProvider
    {
        public DefaultScriptsProvider() : base(Assembly.GetAssembly(typeof(DefaultScriptsProvider)), "Scripts.Defaults")
        {
        }
    }
}
