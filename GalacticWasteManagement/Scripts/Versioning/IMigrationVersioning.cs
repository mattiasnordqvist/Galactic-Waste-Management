using System.Collections.Generic;

namespace GalacticWasteManagement.Scripts
{

    public interface IMigrationVersioning 
    {
        IComparer<IScript> ScriptComparer { get; }
        VersionStringForJournaling VersionStringForJournaling(IScript script);
        int Compare(IScript script, VersionStringForJournaling versionStringForJournaling);
        int Compare(VersionStringForJournaling versionStringForJournaling, VersionStringForJournaling otherVersionStringForJournaling);
    }
}
