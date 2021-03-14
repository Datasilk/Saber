CREATE TABLE [dbo].[User_AuthTokens]
(
	[userId] INT NOT NULL PRIMARY KEY, 
    [token] NVARCHAR(25) NOT NULL, 
    [expires] DATETIME2(7) NOT NULL,
)

GO
