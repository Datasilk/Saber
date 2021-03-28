CREATE PROCEDURE [dbo].[PublicApi_Update]
	@api nvarchar(255),
	@enabled bit
AS
	IF EXISTS(SELECT * FROM PublicApis WHERE api=@api) BEGIN
		UPDATE PublicApis SET [enabled]=@enabled WHERE api=@api
	END ELSE BEGIN
		INSERT INTO PublicApis (api, [enabled]) VALUES (@api, @enabled)
	END
