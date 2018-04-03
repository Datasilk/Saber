CREATE PROCEDURE [dbo].[Page_Title_Delete]
	@titleId int
AS
	DELETE FROM Page_Titles WHERE titleId=@titleId