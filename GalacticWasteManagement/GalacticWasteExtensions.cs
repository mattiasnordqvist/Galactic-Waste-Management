using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JellyDust;
using JellyDust.Dapper;

namespace GalacticWasteManagement
{
    public class GalacticWaste
    {
        public async Task<List<SchemaVersionJournalEntry>> GetSchema(IConnection connection, string schemaVersion, ScriptType type)
        {
            return (await connection.QueryAsync<SchemaVersionJournalEntry>($@"
                SELECT * FROM (
                    SELECT * FROM SchemaVersionJournal WHERE [Type] <> '{nameof(ScriptType.RunIfChanged)}'
                    UNION ALL
                    SELECT * FROM SchemaVersionJournal WHERE Id IN (
                        SELECT Max(Id) FROM SchemaVersionJournal WHERE [Type] = '{nameof(ScriptType.RunIfChanged)}' GROUP BY ScriptName
                    )
                ) _
                WHERE ([Version] = @version OR @version IS NULL) && ([Type] == @type OR @type IS NULL)", new { type, version = schemaVersion })).ToList();
        }
    }
}
