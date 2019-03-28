using System;

namespace GalacticWasteManagement.Scripts
{
    public class DefaultVersion : IComparable<DefaultVersion>
    {
        public int Major { get; set; }
        public int Minor { get; set; }

        public int CompareTo(DefaultVersion other)
        {
            var majorComparison = Major.CompareTo(other.Major);
            if (majorComparison == 0)
            {
                return Minor.CompareTo(other.Minor);
            }

            return majorComparison;
        }
    }
}
