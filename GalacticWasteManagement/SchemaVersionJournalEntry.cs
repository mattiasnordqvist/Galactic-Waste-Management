using System;

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
    }
}
