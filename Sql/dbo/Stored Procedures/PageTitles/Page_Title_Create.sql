CREATE PROCEDURE [dbo].[Page_Title_Create]
	@title nvarchar(MAX),
	@pos bit = 0
AS
	DECLARE @id int = NEXT VALUE FOR SequencePageTitles
	INSERT INTO Page_Titles (titleId, title, pos) VALUES (@id, @title, @pos)

	SELECT @id
