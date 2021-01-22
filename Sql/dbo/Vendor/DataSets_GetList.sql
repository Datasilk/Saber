CREATE PROCEDURE [dbo].[DataSets_GetList]
AS
	SELECT * FROM DataSets 
	WHERE deleted = 0
	ORDER BY tableName ASC
