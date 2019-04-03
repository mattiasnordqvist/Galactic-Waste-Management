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

        public int Compare(Version x, Version y)
        {
            return getVersionToCompare(x).CompareTo(getVersionToCompare(y));
        }
    }
}
