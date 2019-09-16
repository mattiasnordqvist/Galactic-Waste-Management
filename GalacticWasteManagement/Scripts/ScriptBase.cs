using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using GalacticWasteManagement.Utilities;
using JellyDust;
using JellyDust.Dapper;

namespace GalacticWasteManagement.Scripts
{
    public abstract class SqlStringBasedScriptBase : IScript
    {
        private string _cachedHashedContent;
        public SqlStringBasedScriptBase()
        {
            GetHash = () => _cachedHashedContent ?? (_cachedHashedContent = Hashing.CreateHash(Sql));
        }

        public async Task ApplyAsync(ITransaction transaction, Dictionary<string, string> scriptVariables)
        {
            var batches = ScriptUtilities.SplitInBatches(Sql);

            foreach (var batch in batches)
            {
                await transaction.ExecuteScalarAsync(ScriptUtilities.ReplaceTokens(batch, scriptVariables));
            }
        }

        public abstract string Sql { get; }
        public abstract string Name { get; }
        public abstract IScriptType Type { get; }
        public Func<string> GetHash { set; get; }
        
    }
}
