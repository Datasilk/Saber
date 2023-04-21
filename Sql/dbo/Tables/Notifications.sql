CREATE TABLE [dbo].[Notifications]
(
	[notifId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [datecreated] DATETIME2 NOT NULL DEFAULT GETUTCDATE(), 
    [userId] INT NULL, -- optional, only used if notification is for a specific user
    [groupId] INT NULL,-- optional, only used if notification is for a specific security group 
    [securitykey] VARCHAR(32) NULL, -- optional, only used if notification is for users who have access to a specific security key
    [type] VARCHAR(8) NOT NULL,
    [notification] NVARCHAR(128) NOT NULL, 
    [url] NVARCHAR(128) NOT NULL
)
