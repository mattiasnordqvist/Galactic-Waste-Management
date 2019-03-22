using System.Collections.Generic;
using GalacticWasteManagement.Scripts;

namespace GalacticWasteManagement
{
    public class SchemaComparison
    {
        public List<IScript> All { get; set; }
        public List<IScript> New { get; set; }
        public List<(IScript, SchemaVersionJournalEntry)> Changed { get; set; }
        public List<SchemaVersionJournalEntry> Removed { get; set; }
        public List<(IScript, SchemaVersionJournalEntry)> Unchanged { get; set; }
    }
}
