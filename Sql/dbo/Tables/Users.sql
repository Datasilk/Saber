﻿CREATE TABLE [dbo].[Users]
(
	[userId] INT NOT NULL, 
    [name] NVARCHAR(64) NOT NULL, 
    [email] NVARCHAR(64) NOT NULL, 
    [password] NVARCHAR(255) NOT NULL DEFAULT '', 
    [photo] BIT NOT NULL DEFAULT 0, 
    [isadmin] BIT NULL DEFAULT 0,
    [datecreated] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(), 
    [dateactivated] DATETIME2(7) NULL, 
    [keyexpires] DATETIME2(7) NULL, 
    [tempkey] VARCHAR(16) NULL,
    [enabled] BIT NOT NULL DEFAULT 1, 
    PRIMARY KEY ([email])
)

GO

CREATE INDEX [IX_Users_UserId] ON [dbo].[Users] ([userId])

GO

CREATE INDEX [IX_Users_DateCreated] ON [dbo].[Users] ([datecreated])
