CREATE PROCEDURE [dbo].[User_CreateAuthToken]
	@userId int,
	@expireDays int = 30
AS
	DECLARE @id nvarchar(MAX)
	DELETE FROM User_AuthTokens WHERE userId=@userId
	EXEC GetCustomID @length=25, @id = @id OUTPUT, @pattern='?????????????????????????';
	INSERT INTO User_AuthTokens (userId, token, expires)
	VALUES (@userId, @id, DATEADD(DAY, @expireDays, GETUTCDATE()))
	SELECT @id