using System.Collections.Generic;

namespace GalacticWasteManagement.Scripts
{
    public interface IScriptParser
    {
        List<string> SplitInBatches(string script);
    }
}
