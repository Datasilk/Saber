CREATE PROCEDURE [dbo].[User_UpdatePassword]
	@email nvarchar(64),
	@password nvarchar(255)
AS
	UPDATE Users SET [password]=@password WHERE email=@email