CREATE PROCEDURE [dbo].[User_GetDetailsFromEmail]
	@email nvarchar(64)
AS
	SELECT * FROM Users WHERE email=@email
