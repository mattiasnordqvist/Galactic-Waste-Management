using GalacticWasteManagement.Scripts;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GalacticWasteManagement.SqlServer
{
    public class MsSqlScriptParser : IScriptParser
    {
        private readonly TSqlParser _parser;
        private readonly SqlScriptGenerator _sqlScriptGenerator;

        public MsSqlScriptParser(TSqlParser parser, SqlScriptGenerator sqlScriptGenerator)
        {
            _parser = parser;
            _sqlScriptGenerator = sqlScriptGenerator;
        }

        public IList<string> SplitInBatches(string script)
        {
            using (StringReader sr = new StringReader(script))
            {
                var fragment = _parser.Parse(sr, out IList<ParseError> errors);
                if (errors.Any())
                {
                    throw new System.Exception(errors.First().Message);
                }

                return GetBatches(fragment)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();
            }
        }

        private IEnumerable<string> GetBatches(TSqlFragment fragment)
        {
            if (fragment is TSqlScript script)
            {
                foreach (var batch in script.Batches)
                {
                    _sqlScriptGenerator.GenerateScript(batch, out string batchScript);
                    yield return batchScript;
                }
            }
            else
            {
                _sqlScriptGenerator.GenerateScript(fragment, out string fragmentScript);
                yield return fragmentScript;
            }
        }
    }
}
