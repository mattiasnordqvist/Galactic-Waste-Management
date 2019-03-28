using System.Collections.Generic;

namespace GalacticWasteManagement.Scripts
{

    public interface IMigrationVersioning 
    {
        IComparer<IScript> ScriptComparer { get; }
        string DetermineVersion(IScript script);
        int Compare(IScript script, string versionString);
        int Compare(string versionString, string otherVersionString);
    }
}
