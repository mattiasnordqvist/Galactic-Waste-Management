using System;
using System.Collections.Generic;

namespace GalacticWasteManagement.Scripts
{
    public abstract class MigrationVersioning<T> : IComparer<T>, IMigrationVersioning where T : IComparable<T>
    {
        public IComparer<IScript> ScriptComparer => new ScriptComparer<T>(x => DetermineVersion(x));

        public abstract int Compare(T x, T y);

        public abstract T DetermineVersion(IScript script);
        public abstract string ToString(T version);
        public abstract T FromVersionString(string version);

        string IMigrationVersioning.DetermineVersion(IScript script)
        {
            return ToString(DetermineVersion(script));
        }

        public int Compare(IScript x, IScript y)
        {
            return DetermineVersion(x).CompareTo(DetermineVersion(y));
        }

        public int Compare(IScript script, string versionString)
        {
            return DetermineVersion(script).CompareTo(FromVersionString(versionString));
        }

        public int Compare(string versionString, string otherVersionString)
        {
            return FromVersionString(versionString).CompareTo(FromVersionString(otherVersionString));
        }
    }

    internal class ScriptComparer<T> : IComparer<IScript> where T : IComparable<T>
    {
        private Func<IScript, T> p;

        public ScriptComparer(Func<IScript, T> p) 
        {
            this.p = p;
        }

        public int Compare(IScript x, IScript y)
        {
            return p(x).CompareTo(p(y));
        }
    }
}
