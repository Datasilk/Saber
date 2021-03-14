CREATE TABLE [dbo].[User_ApiTokens]
(
	[userId] INT NOT NULL, 
    [api] VARCHAR(32) NOT NULL,
    [token] VARCHAR(16) NOT NULL, 
    [expires] DATETIME2 NOT NULL DEFAULT GETUTCDATE(), 
    CONSTRAINT [PK_User_ApiTokens] PRIMARY KEY ([userId], [api])
)
