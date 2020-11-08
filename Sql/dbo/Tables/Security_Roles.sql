CREATE TABLE [dbo].[Security_Roles]
(
	[userId] INT NOT NULL , 
    [key] VARCHAR(32) NOT NULL, 
    [value] BIT NOT NULL DEFAULT 0, 
    PRIMARY KEY ([userId], [key])
)
