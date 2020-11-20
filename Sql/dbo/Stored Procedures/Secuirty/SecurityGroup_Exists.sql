CREATE PROCEDURE [dbo].[SecurityGroup_Exists]
	@name nvarchar(16)
AS
	SELECT COUNT(*) FROM Security_Groups WHERE [name] = @name
