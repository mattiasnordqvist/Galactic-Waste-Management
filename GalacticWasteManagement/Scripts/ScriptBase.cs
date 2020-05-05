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

        public async Task ApplyAsync(ITransaction transaction, IScriptContext context)
        {
            var batches = context.Parser.SplitInBatches(Sql);

            foreach (var batch in batches)
            {
                await transaction.ExecuteScalarAsync(ReplaceTokens(batch, context.Variables));
            }
        }

        protected virtual string ReplaceTokens(string @this, IDictionary<string, string> variables)
        {
            foreach (var variable in variables)
            {
                @this = @this.Replace($"${variable.Key}$", variable.Value);
            }

            return @this;
        }

        public abstract string Sql { get; }
        public abstract string Name { get; }
        public abstract IScriptType Type { get; }
        public Func<string> GetHash { set; get; }
        
    }
}
