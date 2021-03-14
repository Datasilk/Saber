CREATE PROCEDURE [dbo].[User_AuthenticateApiToken]
	@token nvarchar(16)
AS
	DECLARE @userId int
	SELECT @userId = userId FROM User_ApiTokens WHERE token=@token AND expires > GETUTCDATE()
	IF @userId IS NOT NULL BEGIN
		SELECT * FROM Users WHERE userId = @userId
		AND dateactivated IS NOT NULL AND dateactivated < GETUTCDATE()
	END