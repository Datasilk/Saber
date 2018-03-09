CREATE PROCEDURE [dbo].[Language_Delete]
	@langId nvarchar(2)
AS
	DELETE FROM Languages WHERE langId=@langId