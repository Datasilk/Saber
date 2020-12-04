CREATE PROCEDURE [dbo].[User_Create]
	@name nvarchar(64),
	@email nvarchar(64),
	@password nvarchar(255),
	@photo bit = 0,
	@tempkey varchar(16),
	@activate bit = 0
AS
	DECLARE @id int = NEXT VALUE FOR SequenceUsers
	INSERT INTO Users (userId, [name], email, [password], photo, datecreated, tempkey, keyexpires, dateactivated)
	VALUES (
		@id, @name, @email, @password, @photo, GETDATE(), @tempkey, 
		CASE WHEN @tempkey IS NOT NULL THEN DATEADD(DAY, 1, GETUTCDATE()) ELSE NULL END,
		CASE WHEN @activate = 1 THEN GETUTCDATE() ELSE NULL END
	)
	SELECT @id