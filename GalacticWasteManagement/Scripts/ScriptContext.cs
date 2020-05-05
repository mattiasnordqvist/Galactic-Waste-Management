using System.Collections.Generic;

namespace GalacticWasteManagement.Scripts
{
    public class ScriptContext : IScriptContext
    {
        public ScriptContext(IScriptParser parser, IDictionary<string, string> variables = null)
        {
            Parser = parser;
            Variables = variables ?? new Dictionary<string, string>();
        }

        public IScriptParser Parser { get; }
        public IDictionary<string, string> Variables { get; }
    }
}
