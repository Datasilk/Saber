CREATE PROCEDURE [dbo].[SecurityRoles_GetByUserId]
	@userId int
AS
	SELECT [key], [value] FROM Security_Roles
	WHERE userId=@userId
