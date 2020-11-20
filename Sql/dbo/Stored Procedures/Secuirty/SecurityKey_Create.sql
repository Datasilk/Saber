CREATE PROCEDURE [dbo].[SecurityKey_Create]
	@groupId int,
	@key varchar(32),
	@value bit,
	@isplatform bit
AS
	IF EXISTS(SELECT * FROM Security_Keys WHERE groupId=@groupId AND [key]=@key) BEGIN
		UPDATE Security_Keys SET [value]=@value WHERE groupId=@groupId AND [key]=@key
	END ELSE BEGIN
		INSERT INTO Security_Keys (groupId, [key], [value], isplatform) VALUES (@groupId, @key, @value, @isplatform)
	END
