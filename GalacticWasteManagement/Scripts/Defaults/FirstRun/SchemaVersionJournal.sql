IF OBJECT_ID(N'dbo.SchemaVersionJournal', N'U') IS NULL BEGIN

CREATE TABLE SchemaVersionJournal (
    [Id] int identity(1,1) not null constraint PK_SchemaVersionJournal_Id primary key,
    [Version] nvarchar(255) not null,                    
    [Name] nvarchar(255) not null,
    [Applied] datetime2 not null,
    [Hashed] nvarchar(255) not null,
	[Type] nvarchar(255) not null,
);
END