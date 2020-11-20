CREATE PROCEDURE [dbo].[SecurityGroup_Update]
	@groupId int,
	@name nvarchar(16)
AS
	UPDATE Security_Groups SET [name]=@name WHERE groupId=@groupId
