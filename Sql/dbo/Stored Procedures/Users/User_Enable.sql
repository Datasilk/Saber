CREATE PROCEDURE [dbo].[User_Enable]
	@userId int
AS
	UPDATE Users SET enabled=1 WHERE userId=@userId
