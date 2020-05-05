using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace GalacticWasteManagement.SqlServer
{
    public class MsSql130ScriptParser : MsSqlScriptParser
    {
        public MsSql130ScriptParser()
            : base(new TSql130Parser(false), new Sql130ScriptGenerator())
        {
        }
    }
}
