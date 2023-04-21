CREATE PROCEDURE [dbo].[Notification_Read]
	@notifId uniqueidentifier,
	@userId int
AS
	BEGIN TRY
		INSERT INTO Notifications_Read (notifId, userId) VALUES (@notifId, @userId)
	END TRY
	BEGIN CATCH
	END CATCH
