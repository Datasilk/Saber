﻿CREATE PROCEDURE [dbo].[SecurityRole_Create]
	@userId int,
	@key varchar(32),
	@value bit
AS
	IF EXISTS(SELECT * FROM Security_Roles WHERE userId=@userId AND [key]=@key) BEGIN
		UPDATE Security_Roles SET [value]=@value WHERE userId=@userId AND [key]=@key
	END ELSE BEGIN
		INSERT INTO Security_Roles (userId, [key], [value]) VALUES (@userId, @key, @value)
	END