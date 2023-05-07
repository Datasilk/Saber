CREATE PROCEDURE [dbo].[FileVersion_Update]
	@file varchar(255),
	@version int
AS
	IF EXISTS(SELECT * FROM FileVersions WHERE [file]=@file) BEGIN
		UPDATE FileVersions SET [version]=@version
		WHERE [file]=@file
	END ELSE BEGIN
		INSERT INTO FileVersions ([file], [version])
		VALUES (@file, @version)
	END
