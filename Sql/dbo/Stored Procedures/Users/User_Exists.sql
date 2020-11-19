CREATE PROCEDURE [dbo].[User_Exists]
	@email nvarchar(64)
AS
	SELECT COUNT(*) FROM Users WHERE email=@email
