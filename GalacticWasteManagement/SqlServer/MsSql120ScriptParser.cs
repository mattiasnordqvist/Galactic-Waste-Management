using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace GalacticWasteManagement.SqlServer
{
    public class MsSql120ScriptParser : MsSqlScriptParser
    {
        public MsSql120ScriptParser()
            : base(new TSql120Parser(false), new Sql120ScriptGenerator())
        {
        }
    }
}
