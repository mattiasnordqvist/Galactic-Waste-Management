﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using GalacticWasteManagement.Utilities;
using JellyDust;
using JellyDust.Dapper;

namespace GalacticWasteManagement.Scripts.ScriptProviders
{
    public abstract class ScriptBase : IScript
    {
        private string _cachedHashedContent;

        public async Task ApplyAsync(IConnection connection, Dictionary<string, string> scriptVariables)
        {
            var batches = ScriptUtilities.SplitInBatches(Sql);

            foreach (var batch in batches)
            {
                await connection.ExecuteScalarAsync(ScriptUtilities.ReplaceTokens(batch, scriptVariables));
            }
        }

        public string GetHash()
        {
            if (_cachedHashedContent == null)
            {
                _cachedHashedContent = Hashing.CreateHash(Sql);
            }
            return _cachedHashedContent;
        }

        public abstract string Sql { get; }
        public abstract string Name { get; }
        public abstract ScriptType Type { get; }
    }
}