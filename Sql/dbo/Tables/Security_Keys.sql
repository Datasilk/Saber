CREATE TABLE [dbo].[Security_Keys]
(
	[groupId] INT NOT NULL, 
    [key] VARCHAR(32) NOT NULL, 
    [value] BIT NOT NULL DEFAULT 0, 
    [isplatform] BIT NOT NULL DEFAULT 0, 
    PRIMARY KEY ([groupId], [key])
)
