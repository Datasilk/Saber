CREATE PROCEDURE [dbo].[SecurityUser_Add]
	@groupId int,
	@userId int
AS
	IF NOT EXISTS(SELECT * FROM Security_Users WHERE groupId=@groupId AND userId=@userId) BEGIN
		INSERT INTO Security_Users (groupId, userId) VALUES (@groupId, @userId)
	END
