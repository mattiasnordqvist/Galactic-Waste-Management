using System;
using System.Collections.Generic;

namespace GalacticWasteManagement.Scripts
{
    internal class InternalVersionComparer<TVersion> : IComparer<Version> where TVersion : IComparable<TVersion>
    {
        private readonly Func<Version, TVersion> getVersionToCompare;

        public InternalVersionComparer(Func<Version, TVersion> p) 
        {
            getVersionToCompare = p;
        }

        public int Compare(Version x, Version y) =>
            x != null && y != null
                ? getVersionToCompare(x).CompareTo(getVersionToCompare(y))
                : (x is null ? -1 : 0) + (y is null ? 1 : 0);
    }
}
