CREATE TABLE [dbo].[FileVersions]
(
	[file] VARCHAR(255) NOT NULL PRIMARY KEY, 
    [version] INT NOT NULL DEFAULT 1
)
