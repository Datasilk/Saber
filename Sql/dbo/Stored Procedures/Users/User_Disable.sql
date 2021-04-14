CREATE PROCEDURE [dbo].[User_Disable]
	@userId int
AS
	UPDATE Users SET enabled=0 WHERE userId=@userId
