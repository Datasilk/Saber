CREATE PROCEDURE [dbo].[User_AuthenticateByToken]
	@token nvarchar(25)
AS
	DECLARE @userId int
	SELECT @userId = userId FROM User_AuthTokens WHERE token=@token AND expires > GETUTCDATE()
	DELETE FROM User_AuthTokens WHERE token=@token
	IF @userId IS NOT NULL BEGIN
		DELETE FROM User_AuthTokens WHERE userId=@userId
		SELECT * FROM Users WHERE userId = @userId
		AND dateactivated IS NOT NULL AND dateactivated < GETUTCDATE()
	END