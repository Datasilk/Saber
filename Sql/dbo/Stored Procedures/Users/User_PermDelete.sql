CREATE PROCEDURE [dbo].[User_PermDelete]
	@userId int
AS
	DELETE FROM User_ApiTokens WHERE userId=@userId
	DELETE FROM User_AuthTokens WHERE userId=@userId
	DELETE FROM Security_Users WHERE userId=@userId
	DELETE FROM Users WHERE userId=@userId
