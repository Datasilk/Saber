CREATE PROCEDURE [dbo].[SecurityGroups_GetList]
AS
	SELECT g.*, pk.total AS platformKeys, pk2.total AS pluginKeys, keys.list AS keys
	FROM Security_Groups g
	CROSS APPLY (
		SELECT COUNT(*) AS total FROM Security_Keys
		WHERE groupId=g.groupId
		AND isplatform=1 AND [value]=1
	) as pk
	CROSS APPLY (
		SELECT COUNT(*) AS total FROM Security_Keys
		WHERE groupId=g.groupId
		AND isplatform=0 AND [value]=1
	) as pk2
	CROSS APPLY (
		SELECT TOP 10 STRING_AGG([key], ', ') AS list
		FROM Security_Keys
		WHERE groupId=g.groupId AND [value]=1
	) as keys
	ORDER BY pk.total DESC, g.[name] ASC
