CREATE PROCEDURE [dbo].[User_GetEmailFromResetKey]
	@tempkey varchar(16)
AS
	SELECT email FROM Users WHERE tempkey=@tempkey
