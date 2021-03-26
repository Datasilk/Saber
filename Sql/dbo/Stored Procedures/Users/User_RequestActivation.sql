CREATE PROCEDURE [dbo].[User_RequestActivation]
	@email nvarchar(64),
	@tempkey varchar(16)
AS
	UPDATE Users SET tempkey = @tempkey, keyexpires = DATEADD(DAY, 1, GETUTCDATE())
	WHERE email=@email
