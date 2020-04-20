using System.Collections.Generic;

namespace GalacticWasteManagement.Scripts
{
    public interface IScriptContext
    {
        IScriptParser Parser { get; }
        IDictionary<string, string> Variables { get; }
    }
}
