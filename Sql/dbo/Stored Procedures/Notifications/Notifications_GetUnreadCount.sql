CREATE PROCEDURE [dbo].[Notifications_GetUnreadCount]
	@userId int
AS
	SELECT groupId INTO #groups FROM Security_Users WHERE userId=@userId
	SELECT [key] INTO #keys FROM Security_Keys WHERE groupId IN (SELECT groupId FROM #groups)

	SELECT COUNT(*)
	FROM Notifications n
	LEFT JOIN Notifications_Read r ON r.notifId=n.notifId AND r.userId=@userId
	WHERE r.notifId IS NULL
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
