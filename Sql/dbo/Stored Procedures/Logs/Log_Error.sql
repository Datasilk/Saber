CREATE PROCEDURE [dbo].[Log_Error]
	@userId int,
	@url nvarchar(255),
	@area varchar(64),
	@message varchar(MAX),
	@stacktrace varchar(MAX),
	@data nvarchar(MAX)
AS
	INSERT INTO Log_Errors (datecreated, userId, url, area, message, stacktrace, data)
	VALUES (GETUTCDATE(), @userId, @url, @area, @message, @stacktrace, @data)
