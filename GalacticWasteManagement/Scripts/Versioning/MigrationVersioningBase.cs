using System;
using System.Collections.Generic;

namespace GalacticWasteManagement.Scripts
{
    public abstract class MigrationVersioningBase : IMigrationVersioning
    {
        public int Compare(IScript script, Version version) => Compare(Version(script), version);


        public abstract int Compare(Version version, Version otherVersion);

        /// <summary>
        /// Return a comparer that can compare if a version comes before or after an other version.
        /// </summary>
        public abstract IComparer<Version> VersionComparer { get; }

        /// <summary>
        /// Extract a version from a script.
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public abstract Version Version(IScript script);
    }

    public abstract class CustomVersionMigrationVersioningBase<TCustomVersion> : MigrationVersioningBase  where TCustomVersion : IComparable<TCustomVersion>
    {
        public override int Compare(Version version, Version otherVersion) => VersionComparer.Compare(version, otherVersion);
        public override IComparer<Version> VersionComparer => new InternalVersionComparer<TCustomVersion>(x => FromVersion(x));
        public abstract TCustomVersion ToCustomVersion(IScript script);
        public abstract Version ToVersion(TCustomVersion version);
        public abstract TCustomVersion FromVersion(Version version);
        public override Version Version(IScript script) => ToVersion(ToCustomVersion(script));
    }
}
