CREATE PROCEDURE [dbo].[Session_Get]
	@key varchar(128),
	@expireInMinutes int = 20
AS
	DECLARE @date datetime2 = DATEADD(MINUTE, @expireInMinutes, GETUTCDATE())

	-- flush expired sessions
	DELETE FROM Sessions WHERE Expires < GETUTCDATE()

	-- update requested session expiration
	UPDATE Sessions SET Expires = @date
	WHERE Id=@key

	-- get requested session
	SELECT [Value] FROM Sessions
	WHERE Id=@key
