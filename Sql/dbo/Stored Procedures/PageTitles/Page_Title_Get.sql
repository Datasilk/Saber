CREATE PROCEDURE [dbo].[Page_Title_Get]
	@titleId int
AS
	SELECT title FROM Page_Titles WHERE titleId=@titleId