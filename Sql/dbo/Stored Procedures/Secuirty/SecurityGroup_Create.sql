CREATE PROCEDURE [dbo].[SecurityGroup_Create]
	@name nvarchar(16)
AS
	INSERT INTO Security_Groups ([name]) VALUES (@name)
