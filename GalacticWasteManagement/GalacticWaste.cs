using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalacticWasteManagement.Utilities;
using JellyDust;
using JellyDust.Dapper;

namespace GalacticWasteManagement
{
    public class GalacticWaste
    {
        private readonly IConnection _connection;

        public GalacticWaste(IConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));

        }

        public Task<List<SchemaVersionJournalEntry>> GetSchema(IConnection connection, string schemaVersion = null, ScriptType? type = null)
        {
            return connection.QueryAsync<SchemaVersionJournalEntry>($@"
                SELECT * FROM (
                    SELECT * FROM SchemaVersionJournal WHERE [Type] <> '{nameof(ScriptType.RunIfChanged)}'
                    UNION ALL
                    SELECT * FROM SchemaVersionJournal WHERE Id IN (
                        SELECT Max(Id) FROM SchemaVersionJournal WHERE [Type] = '{nameof(ScriptType.RunIfChanged)}' GROUP BY Name
                    )
                ) _
                WHERE ([Version] = @version OR @version IS NULL) && ([Type] == @type OR @type IS NULL)", new { type, version = schemaVersion })
            .Then(x => Task.FromResult(x.ToList()));
        }

        public Task<SchemaVersionJournalEntry> GetLastSchemaVersionJournalEntry()
        {
            return _connection.QueryAsync<SchemaVersionJournalEntry>("SELECT TOP 1 * FROM SchemaVersion ORDER BY Id")
                .Then(x => Task.FromResult(x.FirstOrDefault()));
        }

        public Task<bool> DbExist(string databaseName)
        {
            var currentDb = _connection.DbConnection.Database;
            _connection.DbConnection.ChangeDatabase("master");
            try
            {
                return _connection.ExecuteScalarAsync<int>("SELECT 1 FROM sys.databases WHERE name = @dbName", new { dbName = databaseName })
                    .Then(x => Task.FromResult(x == 1));
            }
            finally
            {
                _connection.DbConnection.ChangeDatabase(currentDb);
            }
        }
    }
}
