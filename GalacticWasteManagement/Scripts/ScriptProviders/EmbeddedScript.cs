using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using GalacticWasteManagement.Utilities;
using JellyDust;
using JellyDust.Dapper;

namespace GalacticWasteManagement.Scripts.ScriptProviders
{
    public class EmbeddedScript : IScript
    {
        private ResourceFile _resourceFile;
        private string _cachedContent;
        private string _cachedHashedContent;

        public EmbeddedScript(ResourceFile resourceFile, ScriptType type)
        {
            _resourceFile = resourceFile;
            Type = type;
        }

        public string Name => _resourceFile.ResourceKey;

        public string Content
        {
            get
            {
                if (_cachedContent == null)
                {
                    _cachedContent = _resourceFile.Read();
                }
                return _cachedContent;
            }
        }


        public string Hashed
        {
            get
            {
                if (_cachedHashedContent == null)
                {
                    _cachedHashedContent = Hashing.CreateHash(Content);
                }
                return _cachedHashedContent;
            }
        }

        public ScriptType Type { get; private set; }

        public async Task Apply(IConnection connection, Dictionary<string, string> scriptVariables)
        {
            var batches = ScriptUtilities.SplitInBatches(Content);

            foreach (var batch in batches)
            {
                await connection.ExecuteScalarAsync(ScriptUtilities.ReplaceTokens(batch, scriptVariables));
            }
        }
    }
}
