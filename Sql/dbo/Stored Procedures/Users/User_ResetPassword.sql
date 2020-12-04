CREATE PROCEDURE [dbo].[User_ResetPassword]
	@email nvarchar(64),
	@password nvarchar(255),
	@tempkey varchar(16)
AS
	IF EXISTS(SELECT * FROM Users WHERE email=@email AND tempkey = @tempkey AND keyexpires < GETUTCDATE()) BEGIN
		UPDATE Users SET [password]=@password, tempkey = NULL, keyexpires = NULL
		WHERE email=@email
		AND tempkey = @tempkey
		AND keyexpires < GETUTCDATE()
		SELECT 1
	END ELSE BEGIN
		SELECT 0
	END