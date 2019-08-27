using GalacticWasteManagement.Scripts;
using System;

namespace GalacticWasteManagement.Tests.Versioning
{
    public class Semver2Version : DefaultVersion, IComparable<Semver2Version>
    {
        public Semver2Version(string major, string minor, string patch, string pre, string build)
        {
            Major = int.Parse(major);
            Minor = int.Parse(minor);
            Patch = int.Parse(string.IsNullOrEmpty(patch) ? "0" : patch);
            PreRelease = string.IsNullOrEmpty(pre) 
                ? PreReleaseType.None
                : (PreReleaseType)Enum.Parse(typeof(PreReleaseType), pre.TrimStart('-'), true);
            Build = int.Parse(string.IsNullOrEmpty(build) ? "0" : build);
        }

        public int Patch { get; }
        public PreReleaseType PreRelease { get; }
        public int Build { get; }

        public int CompareTo(Semver2Version other)
        {
            var majorMinorComparison = base.CompareTo(other);
            if (majorMinorComparison == 0)
            {
                var patchComparison = Patch.CompareTo(other.Patch);
                if (patchComparison == 0)
                {
                    var preReleaseComparison = PreRelease.CompareTo(other.PreRelease);
                    if (preReleaseComparison == 0)
                    {
                        return Build.CompareTo(other.Build);
                    }
                    else
                    {
                        return preReleaseComparison;
                    }
                }

                return patchComparison;
            }

            return majorMinorComparison;
        }
    }
}
