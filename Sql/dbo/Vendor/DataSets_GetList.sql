CREATE PROCEDURE [dbo].[DataSets_GetList]
	@search nvarchar(MAX)
AS
	SELECT * FROM DataSets 
	WHERE deleted = 0
	AND 
	(
		(
			@search IS NOT NULL
			AND [label] LIKE '%' + @search + '%'
		)
		OR @search IS NULL
	)
	ORDER BY tableName ASC
