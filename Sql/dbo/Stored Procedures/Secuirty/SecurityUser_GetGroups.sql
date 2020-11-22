CREATE PROCEDURE [dbo].[SecurityUser_GetGroups]
	@userId int
AS
	SELECT sg.* 
	FROM Security_Groups sg
	JOIN Security_Users su ON su.userId=@userId
	WHERE sg.groupId = su.groupId
	
