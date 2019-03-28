 CREATE TABLE [dbo].[User]( 
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](256) NOT NULL, 
	[Name] [nvarchar](255) NOT NULL,
	[Email] [nvarchar](255) NULL,
	[Password] varbinary(max) NULL,
	[Salt] varbinary(max) NULL,	
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[EventLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Text] [nvarchar](256) NOT NULL,
	[User] [nvarchar](256) NOT NULL,
	[Timestamp] [datetime] NOT NULL,
 CONSTRAINT [PK_Log] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[ApiClient](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Secret] [nvarchar](255) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[ApplicationType] [bit] NOT NULL,
	[Active] [bit] NOT NULL,
	[RefreshTokenLifeTime] [int] NOT NULL,
	[AllowedOrigin] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_ApiClient] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

INSERT INTO ApiClient (Secret, Name, ApplicationType, Active, RefreshTokenLifeTime, AllowedOrigin) VALUES ('', 'spinit.stack-web', 0, 1, 7200, '*')

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RefreshToken](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[HashedRefreshTokenId] [nvarchar](255) NOT NULL,
	[Subject] [nvarchar](255) NOT NULL,
	[ApiClient_Id] [int] NOT NULL,
	[IssuedUtc] [datetime] NOT NULL,
	[ExpiresUtc] [datetime] NOT NULL,
	[ProtectedTicket] [nvarchar](1024) NOT NULL,
 CONSTRAINT [PK_RefreshToken] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[RefreshToken]  WITH CHECK ADD  CONSTRAINT [FK_RefreshToken_ApiClient] FOREIGN KEY([ApiClient_Id])
REFERENCES [dbo].[ApiClient] ([Id])
GO

ALTER TABLE [dbo].[RefreshToken] CHECK CONSTRAINT [FK_RefreshToken_ApiClient]
GO

CREATE TABLE [dbo].[Role](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[IsSystem] [bit] NOT NULL,
 CONSTRAINT [PK_Role] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Role] ADD  DEFAULT ((0)) FOR [IsSystem]
GO

INSERT INTO [Role] (Name, IsSystem) VALUES ('system developer', 1)
GO


CREATE TABLE [dbo].[Role_User](
	[Role_Id] [int] NOT NULL,
	[User_Id] [int] NOT NULL,
 CONSTRAINT [PK_Role_User] PRIMARY KEY CLUSTERED 
(
	[Role_Id] ASC,
	[User_Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Role_User]  WITH CHECK ADD  CONSTRAINT [FK_Role_User_Role] FOREIGN KEY([Role_Id])
REFERENCES [dbo].[Role] ([Id])
GO

ALTER TABLE [dbo].[Role_User] CHECK CONSTRAINT [FK_Role_User_Role]
GO

ALTER TABLE [dbo].[Role_User]  WITH CHECK ADD  CONSTRAINT [FK_Role_User_User] FOREIGN KEY([User_Id])
REFERENCES [dbo].[User] ([Id])
GO

ALTER TABLE [dbo].[Role_User] CHECK CONSTRAINT [FK_Role_User_User]
GO