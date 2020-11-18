CREATE PROCEDURE [dbo].[Users_GetList]
	@page int = 1,
	@length int = 10,
	@search nvarchar(MAX) = '',
	@orderby int = 0
AS
BEGIN
	SET NOCOUNT ON;

	SELECT u.*, sec.total AS [security]
	FROM Users u
	
	CROSS APPLY (
		SELECT 
		CASE WHEN u.userId = 1 THEN 999
		ELSE COUNT(*) END AS total
		FROM Security_Roles sr
		WHERE sr.userId = u.userId
		AND sr.isplatform = 1
	) AS sec

	WHERE 
	(
		(@search <> '' AND 
			(
				u.[name] LIKE '%' + @search + '%'
				OR u.email LIKE '%' + @search + '%'
			)
			OR @search = ''
		)
	)
	ORDER BY sec.total DESC,
	CASE WHEN @orderby = 0 THEN u.userId END ASC,
	CASE WHEN @orderby = 1 THEN u.email END ASC,
	CASE WHEN @orderby = 2 THEN u.datecreated END DESC
	OFFSET @length * (@page - 1) ROWS FETCH NEXT @length ROWS ONLY
END