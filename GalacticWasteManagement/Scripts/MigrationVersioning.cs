using System.Collections.Generic;

namespace GalacticWasteManagement.Scripts
{
    public abstract class MigrationVersioning<T> : IComparer<T>, IMigrationVersioning
    {
        public abstract int Compare(T x, T y);

        public abstract T DetermineVersion(IScript script);
        public abstract string ToString(T version);
        public abstract T FromString(string version);

        string IMigrationVersioning.DetermineVersion(IScript script)
        {
            return ToString(DetermineVersion(script));
        }

        int IMigrationVersioning.Compare(string versionX, string versionY)
        {
            return Compare(FromString(versionX), FromString(versionY));
        }

        public int Compare(IScript versionX, string versionY)
        {
            return Compare(DetermineVersion(versionX), FromString(versionY));
        }
    }
}
