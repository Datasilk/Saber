CREATE PROCEDURE [dbo].[User_GetDetails]
	@userId int
AS
	SELECT * FROM Users WHERE userId=@userId
