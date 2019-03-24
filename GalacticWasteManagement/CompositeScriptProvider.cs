using System.Collections.Generic;
using System.Linq;
using GalacticWasteManagement.Scripts;

namespace GalacticWasteManagement
{
    internal class CompositeScriptProvider : IScriptProvider
    {
        private readonly IScriptProvider[] scriptProviders;

        public CompositeScriptProvider(params IScriptProvider[] scriptProviders)
        {
            this.scriptProviders = scriptProviders;
        }

        public IEnumerable<IScript> GetScripts(ScriptType scriptType)
        {
            return scriptProviders.SelectMany(x => x.GetScripts(scriptType));
        }
    }
}