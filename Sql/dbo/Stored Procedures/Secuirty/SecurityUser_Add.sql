CREATE PROCEDURE [dbo].[SecurityUser_Add]
	@groupId int,
	@userId int
AS
	INSERT INTO Security_Users (groupId, userId) VALUES (@groupId, @userId)
