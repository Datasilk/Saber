CREATE PROCEDURE [dbo].[Notifications_GetList]
	@userId int,
	@lastChecked datetime2(7),
	@length int = 10
AS
	SELECT groupId INTO #groups FROM Security_Users WHERE userId=@userId
	SELECT [key] INTO #keys FROM Security_Keys WHERE groupId IN (SELECT groupId FROM #groups)

	SELECT n.*, (CASE WHEN r.userId IS NOT NULL THEN 1 ELSE 0 END) AS [read]
	FROM Notifications n
	LEFT JOIN Notifications_Read r ON r.notifId=n.notifId AND r.userId=@userId
	WHERE (
		(@lastChecked IS NOT NULL AND n.datecreated > @lastChecked)
		OR @lastChecked IS NULL
	)
	AND (
		(n.userId IS NOT NULL AND n.userId = @userId)
		OR n.userId IS NULL
	)
	AND (
		(n.groupId IS NOT NULL AND n.groupId IN (SELECT groupId FROM #groups))
		OR n.groupId IS NULL
	)
	AND (
		(n.securitykey IS NOT NULL AND n.securitykey <> '' AND n.securitykey IN (SELECT [key] FROM #keys))
		OR n.securitykey IS NULL
		OR n.securitykey = ''
	)
	ORDER BY n.datecreated DESC
	OFFSET 0 ROWS FETCH NEXT @length ROWS ONLY
