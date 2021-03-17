CREATE PROCEDURE [dbo].[DataSet_Delete]
	@datasetId int
AS
	DECLARE @tableName nvarchar(64)
	SELECT @tableName=tableName FROM DataSets WHERE datasetId=@datasetId
	DECLARE @sql nvarchar(MAX) = 'DROP TABLE DataSet_' + @tableName
	EXEC sp_executesql @sql
	SET @sql = 'DROP SEQUENCE Sequence_DataSet_' + @tableName
	EXEC sp_executesql @sql
	DELETE FROM DataSets WHERE datasetId=@datasetId

