CREATE PROCEDURE [dbo].[Log_Errors_GetList]
	@start int = 0,
	@length int = 10,
	@search nvarchar(MAX) = NULL

AS
	SELECT * FROM Log_Errors e
	WHERE (
		(
			@search IS NOT NULL AND @search <> '' AND 
			(
				e.area LIKE '%' + @search + '%'
				OR e.[message] LIKE '%' + @search + '%'
				OR e.stacktrace LIKE '%' + @search + '%'
			)
		)
		OR @search IS NULL OR @search = ''
	)
	ORDER BY e.datecreated DESC
	OFFSET @start ROWS FETCH NEXT @length ROWS ONLY
