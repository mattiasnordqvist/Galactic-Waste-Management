using System.Collections.Generic;

namespace GalacticWasteManagement.Scripts
{
    public interface IScriptParser
    {
        IList<string> SplitInBatches(string script);
    }
}
