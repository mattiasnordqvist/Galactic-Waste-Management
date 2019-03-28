using System;
using System.Collections.Generic;

namespace GalacticWasteManagement.Scripts
{
    public abstract class MigrationVersioningBase : IMigrationVersioning
    {
        public abstract IComparer<IScript> ScriptComparer { get; }
        public int Compare(IScript script, VersionStringForJournaling versionStringForJournaling)
        {
            return Compare(VersionStringForJournaling(script), versionStringForJournaling);
        }

        public abstract int Compare(VersionStringForJournaling versionStringForJournaling, VersionStringForJournaling otherVersionStringForJournaling);
        public abstract VersionStringForJournaling VersionStringForJournaling(IScript script);
    }

    public abstract class MigrationVersioningBase<T> : MigrationVersioningBase, IComparer<T>  where T : IComparable<T>
    {
        public override IComparer<IScript> ScriptComparer => new ScriptComparer<T>(x => GetVersion(x));

        public abstract int Compare(T x, T y);
        public abstract T GetVersion(IScript script);
        public abstract VersionStringForJournaling ToVersionStringForJournaling(T version);
        public abstract T FromVersionStringForJournaling(VersionStringForJournaling versionStringForJournaling);

        public override VersionStringForJournaling VersionStringForJournaling(IScript script)
        {
            return ToVersionStringForJournaling(GetVersion(script));
        }

        public int Compare(IScript x, IScript y)
        {
            return GetVersion(x).CompareTo(GetVersion(y));
        }

        public override int Compare(VersionStringForJournaling versionStringForJournaling, VersionStringForJournaling otherVersionStringForJournaling)
        {
            return FromVersionStringForJournaling(versionStringForJournaling).CompareTo(FromVersionStringForJournaling(otherVersionStringForJournaling));
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
