CREATE PROCEDURE [dbo].[User_Activate]
	@email nvarchar(64),
	@tempkey varchar(16)
AS
	IF EXISTS(SELECT * FROM Users WHERE email=@email AND tempkey=@tempkey AND keyexpires < GETUTCDATE()) BEGIN
		UPDATE Users SET tempkey = NULL, keyexpires = NULL, dateactivated = GETUTCDATE()
		WHERE email = @email
		SELECT 1
	END ELSE BEGIN
		SELECT 0
	END
