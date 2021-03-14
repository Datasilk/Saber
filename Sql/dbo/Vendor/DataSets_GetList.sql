CREATE PROCEDURE [dbo].[DataSets_GetList]
	@userId int NULL = NULL,
	@all bit = 0,
	@search nvarchar(MAX)
AS
	DECLARE @isadmin bit = 0
	IF @userId IS NOT NULL BEGIN
		--check if user is admin
		SET @isadmin = CASE WHEN EXISTS(SELECT * FROM Users WHERE userId=@userId AND isadmin=1) THEN 1 ELSE 0 END
	END
	SELECT * FROM DataSets 
	WHERE deleted = 0
	AND
	(
		-- user permissions
		(
			@userId IS NOT NULL
			AND (
				(@all = 1 AND (userId IS NULL OR userId = @userId))
				OR (@all = 0 AND userId = @userId)
			)
		)
		OR
		(
			@isadmin = 1 --Admin account
			AND @all = 1
		)
		OR
		(
			@userId IS NULL 
			AND userId IS NULL
		)
	)
	AND 
	(
		--  text search
		(
			@search IS NOT NULL
			AND [label] LIKE '%' + @search + '%'
		)
		OR @search IS NULL
	)
	ORDER BY tableName ASC
