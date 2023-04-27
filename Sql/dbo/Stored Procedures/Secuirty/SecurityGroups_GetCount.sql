CREATE PROCEDURE [dbo].[SecurityGroups_GetCount]
AS
	SELECT COUNT(*) FROM Security_Groups g
	
