using System.Collections.Generic;

namespace GalacticWasteManagement.Scripts.ScriptProviders
{
    public class BuiltInScriptsScriptProvider : IScriptProvider
    {
        public IEnumerable<IScript> GetScripts(ScriptType scriptType)
        {
            if(scriptType == ScriptType.Create)
            {
                yield return new CreateDatabase();
            }
            if (scriptType == ScriptType.FirstRun)
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
