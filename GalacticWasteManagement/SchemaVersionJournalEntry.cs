using System;
using GalacticWasteManagement.Scripts;

namespace GalacticWasteManagement
{
    public class SchemaVersionJournalEntry
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Applied { get; set; }
        public string Version { get; set; }
        public string Hash { get; set; }
        public string Type { get; set; }
        public VersionStringForJournaling VersionStringForJournaling { get { return new VersionStringForJournaling(Version); } }
    }
}
