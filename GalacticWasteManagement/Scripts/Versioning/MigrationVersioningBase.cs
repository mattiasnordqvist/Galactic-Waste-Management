using System;
using System.Collections.Generic;

namespace GalacticWasteManagement.Scripts
{
    public abstract class MigrationVersioningBase : IMigrationVersioning
    {
        public int Compare(IScript script, Version versionStringForJournaling) => Compare(VersionStringForJournaling(script), versionStringForJournaling);
        
        /// <summary>
        /// Return a comparer that can compare if a version comes before or after an other version.
        /// </summary>
        public abstract IComparer<Version> VersionComparer { get; }

        public int Compare(Version versionStringForJournaling, Version otherVersionStringForJournaling)
        {
            return VersionComparer.Compare(versionStringForJournaling, otherVersionStringForJournaling);
        }

        /// <summary>
        /// Extract a version from a script.
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public abstract Version VersionStringForJournaling(IScript script);
    }

    public abstract class MigrationVersioningBase<TVersion> : MigrationVersioningBase, IComparer<TVersion>  where TVersion : IComparable<TVersion>
    {
        public override IComparer<Version> VersionComparer => new InternalVersionComparer<TVersion>(x => FromVersionStringForJournaling(x));
        public abstract int Compare(TVersion x, TVersion y);
        public abstract TVersion GetVersion(IScript script);
        public abstract Version ToVersionStringForJournaling(TVersion version);
        public abstract TVersion FromVersionStringForJournaling(Version versionStringForJournaling);
        public override Version VersionStringForJournaling(IScript script) => ToVersionStringForJournaling(GetVersion(script));
    }
}
