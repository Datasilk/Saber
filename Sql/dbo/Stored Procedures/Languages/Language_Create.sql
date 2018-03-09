CREATE PROCEDURE [dbo].[Language_Create]
	@langId nvarchar(2),
	@language nvarchar(25)
AS
	INSERT INTO Languages (langId, language) VALUES (@langId, @language)