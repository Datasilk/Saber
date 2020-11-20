CREATE PROCEDURE [dbo].[SecurityGroup_Delete]
	@groupId int
AS
	DELETE FROM Security_Users WHERE groupId=@groupId
	DELETE FROM Security_Keys WHERE groupId=@groupId
	DELETE FROM Security_Groups WHERE groupId=@groupId
