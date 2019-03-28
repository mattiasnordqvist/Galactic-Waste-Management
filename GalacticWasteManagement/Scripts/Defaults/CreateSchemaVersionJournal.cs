namespace GalacticWasteManagement.Scripts.ScriptProviders
{
    public class CreateSchemaVersionJournal : ScriptBase
    {
        public override string Name => nameof(CreateSchemaVersionJournal);

        public override IScriptType Type => ScriptType.Initialize;

        public override string Sql => @"
IF OBJECT_ID(N'dbo.SchemaVersionJournal', N'U') IS NULL BEGIN

CREATE TABLE SchemaVersionJournal (
    [Id] int identity(1,1) not null constraint PK_SchemaVersionJournal_Id primary key,
    [Version] nvarchar(255) not null,                    
    [Name] nvarchar(255) not null,
    [Applied] datetime2 not null,
    [Hash] nvarchar(255) not null,
	[Type] nvarchar(255) not null,
);
END";
    }
}
