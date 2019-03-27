namespace GalacticWasteManagement.Scripts.ScriptProviders
{
    public class CreateDatabase : ScriptBase
    {
        public override string Name => nameof(CreateDatabase);

        public override ScriptType Type => ScriptType.Create;

        public override string Sql => @"
IF(DB_ID(N'$DbName$') IS NULL)
BEGIN
    CREATE DATABASE [$DbName$]
END";
    }
}
