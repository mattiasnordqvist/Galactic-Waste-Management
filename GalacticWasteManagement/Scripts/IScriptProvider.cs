﻿using System.Collections.Generic;

namespace GalacticWasteManagement.Scripts
{
    public interface IScriptProvider
    {
        IEnumerable<IScript> GetScripts(IScriptType scriptType);
    }
}
