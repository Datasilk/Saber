CREATE PROCEDURE [dbo].[User_Activate]
	@tempkey varchar(16)
AS
	IF EXISTS(SELECT * FROM Users WHERE tempkey=@tempkey AND keyexpires > GETUTCDATE()) BEGIN
		UPDATE Users SET tempkey = NULL, keyexpires = NULL, dateactivated = GETUTCDATE()
		WHERE tempkey=@tempkey
		SELECT 1
	END ELSE BEGIN
		SELECT 0
	END
