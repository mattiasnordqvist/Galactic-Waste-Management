using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace GalacticWasteManagement.SqlServer
{
    public class MsSql140ScriptParser : MsSqlScriptParser
    {
        public MsSql140ScriptParser()
            : base(new TSql140Parser(false), new Sql140ScriptGenerator())
        {
        }
    }
}
