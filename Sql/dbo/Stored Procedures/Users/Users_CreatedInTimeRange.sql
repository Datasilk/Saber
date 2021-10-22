CREATE PROCEDURE [dbo].[Users_CreatedInTimeRange]
	@minutes int = 60,
	@dateend datetime = NULL
AS
	SELECT COUNT(*) FROM Users WHERE datecreated >= DATEADD(MINUTE, @minutes * -1, ISNULL(@dateend, GETUTCDATE()))
