CREATE PROCEDURE [dbo].[Log_ForgotPassword]
	@email nvarchar(64)
AS
	INSERT INTO Log_ForgotPasswords (email) VALUES(@email)

