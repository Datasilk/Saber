CREATE PROCEDURE [dbo].[User_ResetPassword]
	@password nvarchar(255),
	@tempkey varchar(16)
AS
	IF EXISTS(SELECT * FROM Users WHERE tempkey = @tempkey AND keyexpires > GETUTCDATE()) BEGIN
		UPDATE Users SET [password]=@password, tempkey = NULL, keyexpires = NULL
		WHERE tempkey = @tempkey
		AND keyexpires > GETUTCDATE()
		SELECT 1
	END ELSE BEGIN
		SELECT 0
	END