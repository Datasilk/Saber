CREATE PROCEDURE [dbo].[User_RequestActivation]
	@userId int,
	@tempkey varchar(16)
AS
	UPDATE Users SET tempkey = @tempkey, keyexpires = DATEADD(DAY, 1, GETUTCDATE())
	WHERE userId = @userId
