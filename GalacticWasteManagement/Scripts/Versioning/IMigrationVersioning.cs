using System.Collections.Generic;

namespace GalacticWasteManagement.Scripts
{

    public interface IMigrationVersioning 
    {
        IComparer<Version> VersionComparer { get; }
        Version VersionStringForJournaling(IScript script);
        int Compare(IScript script, Version versionStringForJournaling);
        int Compare(Version versionStringForJournaling, Version otherVersionStringForJournaling);
    }
}
