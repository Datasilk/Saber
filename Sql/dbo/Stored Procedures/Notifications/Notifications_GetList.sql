CREATE PROCEDURE [dbo].[Notifications_GetList]
	@userId int,
	@lastChecked datetime2(7),
	@length int = 10
AS
	SELECT n.*, (CASE WHEN r.userId IS NOT NULL THEN 1 ELSE 0 END) AS [read]
	FROM Notifications n
	LEFT JOIN Notifications_Read r ON r.notifId=n.notifId AND r.userId=@userId
	WHERE n.datecreated > @lastChecked
	ORDER BY n.datecreated DESC
	OFFSET 0 ROWS FETCH NEXT @length ROWS ONLY
