CREATE PROCEDURE [dbo].[User_UpdateAdmin]
	@userId int,
	@isadmin bit
AS
	UPDATE Users SET isadmin=@isadmin WHERE userId=@userId
