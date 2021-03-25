CREATE PROCEDURE [dbo].[DataSet_GetColumns]
	@datasetId int
AS
	DECLARE @tableName nvarchar(64)
	SELECT @tableName = tableName FROM DataSets WHERE datasetId=@datasetId
	SELECT c.[name] AS [Name]
	FROM sys.columns c
	WHERE c.object_id = OBJECT_ID('DataSet_' + @tableName)
	AND c.[name] NOT IN ('id', 'lang', 'userId')
