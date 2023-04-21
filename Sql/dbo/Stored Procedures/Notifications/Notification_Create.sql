CREATE PROCEDURE [dbo].[Notification_Create]
	@userId int NULL,
	@groupId int NULL,
	@securitykey varchar(32) NULL,
	@type varchar(8),
	@notification nvarchar(128),
	@url nvarchar(128)
AS
	INSERT INTO Notifications (notifId, datecreated, userId, groupId, securitykey, [type], [notification], [url])
	VALUES (NEWID(), GETUTCDATE(), @userId, @groupId, @securitykey, @type, @notification, @url)
