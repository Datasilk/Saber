CREATE PROCEDURE [dbo].[SecurityKeys_GetByUserId]
	@userId int
AS
	SELECT DISTINCT k.[key], k.[value] 
	FROM Security_Keys k
	JOIN Security_Users u ON u.userId = @userId AND k.groupId = u.groupId
