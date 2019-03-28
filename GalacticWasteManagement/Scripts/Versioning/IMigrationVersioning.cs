using System.Collections.Generic;

namespace GalacticWasteManagement.Scripts
{

    public interface IMigrationVersioning : IComparer<Version>
    {
        IComparer<Version> VersionComparer { get; }
        Version Version(IScript script);
    }
}
