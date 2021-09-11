CREATE PROCEDURE [dbo].[Session_Set]
	@key varchar(128),
	@value nvarchar(MAX),
	@expireInMinutes int = 20
AS
	DECLARE @date datetime2 = DATEADD(MINUTE, @expireInMinutes, GETUTCDATE())

	-- flush expired sessions
	DELETE FROM Sessions WHERE Expires < GETUTCDATE()

	BEGIN TRY
		INSERT INTO Sessions (Id, [Value], Expires) VALUES (@key, @value, @date)
	END TRY
	BEGIN CATCH
		UPDATE Sessions SET [Value] = @value, Expires = @date 
		WHERE Id = @key
	END CATCH