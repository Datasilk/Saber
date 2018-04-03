CREATE PROCEDURE [dbo].[Page_Titles_GetList]
	@pos int = 0
AS
	IF @pos > 2 OR @pos < 0 BEGIN
		SELECT * FROM Page_Titles ORDER BY pos ASC, titleId ASC
	END ELSE BEGIN
		SELECT * FROM Page_Titles WHERE pos=@pos ORDER BY titleId ASC
	END
	
