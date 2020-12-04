CREATE TABLE [dbo].[Users]
(
	[userId] INT NOT NULL, 
    [name] NVARCHAR(64) NOT NULL, 
    [email] NVARCHAR(64) NOT NULL, 
    [password] NVARCHAR(255) NOT NULL DEFAULT '', 
    [photo] BIT NOT NULL DEFAULT 0, 
    [datecreated] DATETIME NOT NULL DEFAULT GETDATE(), 
    [dateactivated] DATETIME NULL, 
    [keyexpires] DATETIME NULL, 
    [tempkey] VARCHAR(16) NULL,
    PRIMARY KEY ([email])
)

GO

CREATE INDEX [IX_Users_UserId] ON [dbo].[Users] ([userId])

GO

CREATE INDEX [IX_Users_DateCreated] ON [dbo].[Users] ([datecreated])
