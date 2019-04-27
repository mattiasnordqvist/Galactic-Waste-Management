using System.Collections.Generic;

namespace GalacticWasteManagement.Scripts.ScriptProviders
{
    public class BuiltInScriptsScriptProvider : IScriptProvider
    {
        public IEnumerable<IScript> GetScripts(IScriptType scriptType)
        {
            if (scriptType == ScriptType.Initialize)
            {
                yield return new CreateSchemaVersionJournal();
            }
            if (scriptType == ScriptType.Drop)
            {
                yield return new DropSchema();
            }
        }
    }
}
