CREATE PROCEDURE [dbo].[User_Authenticate] 
	@email nvarchar(64) = '',
	@password nvarchar(255) = ''
AS
BEGIN
	SELECT u.*
	FROM Users u
	WHERE u.email=@email AND u.[password]=@password
	AND u.dateactivated IS NOT NULL AND u.dateactivated < GETUTCDATE()
END