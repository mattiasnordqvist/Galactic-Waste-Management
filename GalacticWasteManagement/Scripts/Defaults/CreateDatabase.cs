namespace GalacticWasteManagement.Scripts.ScriptProviders
{
    public class CreateDatabase : ScriptBase
    {
        public override string Name => nameof(CreateDatabase);

        public override IScriptType Type => ScriptType.Create;

        public override string Sql => @"
IF(DB_ID(N'$DbName$') IS NULL)
BEGIN
    CREATE DATABASE [$DbName$]
END";
    }
}
