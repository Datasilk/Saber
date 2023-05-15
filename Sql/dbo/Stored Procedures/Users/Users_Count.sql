CREATE PROCEDURE [dbo].[Users_Count]
	@search nvarchar(MAX) = ''
AS
BEGIN
	SET NOCOUNT ON;

	SELECT COUNT(*)
	FROM Users u
	WHERE 
	(
		(@search <> '' AND 
			( 
				u.[name] LIKE '%' + @search + '%'
				OR u.email LIKE '%' + @search + '%'
			)
		) OR @search = ''
	)
END