using GalacticWasteManagement.Scripts;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GalacticWasteManagement.SqlServer
{
    public class MsSqlScriptParser : IScriptParser
    {
        public List<string> SplitInBatches(string script)
        {
            var parser = new TSql120Parser(false);
            using (StringReader sr = new StringReader(script))
            {
                var fragment = parser.Parse(sr, out IList<ParseError> errors);
                if (errors.Any())
                {
                    throw new System.Exception(errors.First().Message);
                }

                return GetBatches(fragment).ToList();
            }
        }

        private static IEnumerable<string> GetBatches(TSqlFragment fragment)
        {
            var sg = new Sql120ScriptGenerator();
            if (fragment is TSqlScript script)
            {
                foreach (var batch in script.Batches)
                {
                    yield return ScriptFragment(sg, batch);
                }
            }
            else
            {
                yield return ScriptFragment(sg, fragment);
            }
        }

        private static string ScriptFragment(SqlScriptGenerator sg, TSqlFragment fragment)
        {
            sg.GenerateScript(fragment, out string resultString);
            return resultString;
        }
    }
}
