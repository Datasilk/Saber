CREATE PROCEDURE [dbo].[SecurityUser_Remove]
	@groupId int,
	@userId int
AS
	DELETE FROM Security_Users WHERE groupId=@groupId AND userId=@userId

