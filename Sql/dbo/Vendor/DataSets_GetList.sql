CREATE PROCEDURE [dbo].[DataSets_GetList]
AS
	SELECT * FROM DataSets ORDER BY tableName ASC
