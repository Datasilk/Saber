CREATE PROCEDURE [dbo].[SecurityKeys_GetList]
	@groupId int
AS
	SELECT * FROM Security_Keys WHERE groupId=@groupId AND [value] = 1
