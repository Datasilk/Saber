CREATE PROCEDURE [dbo].[User_ForgotPassword]
	@email nvarchar(64),
	@tempkey varchar(16)
AS
	IF EXISTS(SELECT * FROM Users WHERE email=@email AND dateactivated IS NOT NULL) BEGIN
		UPDATE Users SET tempkey=@tempkey, keyexpires = DATEADD(DAY, 1, GETUTCDATE())
		WHERE email=@email
		SELECT 1
	END ELSE BEGIN
		SELECT 0
	END