CREATE TABLE [dbo].[DataSets]
(
	[datasetId] INT IDENTITY(1,1) PRIMARY KEY, 
    [userId] INT NULL, 
    [label] NVARCHAR(64) NOT NULL, 
    [tableName] NVARCHAR(64) NOT NULL, 
    [partialview] NVARCHAR(255) NOT NULL DEFAULT '', 
    [datecreated] DATETIME2 NOT NULL, 
    [description] NVARCHAR(MAX) NOT NULL, 
    [deleted] BIT NOT NULL DEFAULT 0
)

GO

CREATE INDEX [IX_DataSets_TableName] ON [dbo].[DataSets] ([tableName])
