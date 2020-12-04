CREATE PROCEDURE [dbo].[User_CanActivate]
	@email nvarchar(64)
AS
	IF EXISTS(SELECT * FROM Users WHERE email=@email AND dateactivated IS NULL) BEGIN
		SELECT 1
	END ELSE BEGIN
		SELECT 0
	END
