using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace GalacticWasteManagement.SqlServer
{
    public class MsSql150ScriptParser : MsSqlScriptParser
    {
        public MsSql150ScriptParser()
            : base(new TSql150Parser(false), new Sql150ScriptGenerator())
        {
        }
    }
}
