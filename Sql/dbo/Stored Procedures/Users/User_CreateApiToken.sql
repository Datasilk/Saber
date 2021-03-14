CREATE PROCEDURE [dbo].[User_CreateApiToken]
	@userId int,
	@api varchar(32),
	@expireDays int = 730
AS
	DECLARE @id nvarchar(MAX)
	DELETE FROM User_ApiTokens WHERE userId=@userId AND api=@api
	EXEC GetCustomID @length=16, @id = @id OUTPUT, @pattern='????????????????';
	INSERT INTO User_ApiTokens (userId, api, token, expires)
	VALUES (@userId, @api, @id, DATEADD(DAY, @expireDays, GETUTCDATE()))
	SELECT @id
