using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GalacticWasteManagement.Utilities;

namespace GalacticWasteManagement.Scripts
{
    public class EmbeddedScriptProvider : IScriptProvider
    {
        private readonly Assembly _scriptsAssembly;
        private readonly string _scriptsRootFolder;
        private string _namespacePrefix;

        public EmbeddedScriptProvider(Assembly scriptsAssembly, string scriptsRootFolder)
        {
            _namespacePrefix = scriptsAssembly.GetName().Name;
            _scriptsAssembly = scriptsAssembly;
            _scriptsRootFolder = scriptsRootFolder;
        }

        public IEnumerable<IScript> GetScripts(string type)
        {
            return EmbeddedResourceReader.GetResourcesFrom(_scriptsAssembly, x => x.StartsWith($"{_namespacePrefix}.{_scriptsRootFolder}.{type}"))
                .Select(x => new EmbeddedScript(x, type)).ToList();
        }

    }
}
