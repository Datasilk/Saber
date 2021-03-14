CREATE PROCEDURE [dbo].[Log_GetUrlAnalytics]
	@timeScale int = 1, -- 0 = 60 minutes, 1 = 24 hours, 2 = 7 days, 3 = 30 days, 4 = 12 months
	@startDate datetime2(7) = NULL
AS
	IF @startDate = NULL BEGIN SET @startDate = GETUTCDATE() END

	SELECT TOP (
		CASE WHEN @timeScale = 0 THEN 60 ELSE
		CASE WHEN @timeScale = 1 THEN 24 ELSE
		CASE WHEN @timeScale = 2 THEN (7 * 24) ELSE
		CASE WHEN @timeScale = 3 THEN 30 ELSE
		CASE WHEN @timeScale = 4 THEN 12 ELSE 12
		 END END END END END
	) COUNT(*) AS total,
	CONVERT(char(4), YEAR(r.datecreated)) + '-' + FORMAT(r.datecreated, 'MM') +
		CASE WHEN @timeScale < 4 THEN '-' + FORMAT(r.datecreated, 'dd') ELSE '' END +
		CASE WHEN @timeScale < 2 THEN ' ' + FORMAT(r.datecreated, 'hh') ELSE '' END +
		CASE WHEN @timeScale < 1 THEN ':' + FORMAT(r.datecreated, 'mm') ELSE '' END
		AS datesort, MIN(r.datecreated) AS datecreated,
		u.urlId, u.[url]
	FROM Log_UrlRequests r
	JOIN Log_Urls u ON u.urlId = r.urlId
	WHERE r.datecreated >= @startDate
	GROUP BY CONVERT(char(4), YEAR(r.datecreated)) + '-' + FORMAT(r.datecreated, 'MM') +
		CASE WHEN @timeScale < 4 THEN '-' + FORMAT(r.datecreated, 'dd') ELSE '' END +
		CASE WHEN @timeScale < 2 THEN ' ' + FORMAT(r.datecreated, 'hh') ELSE '' END +
		CASE WHEN @timeScale < 1 THEN ':' + FORMAT(r.datecreated, 'mm') ELSE '' END,
	u.urlId, u.[url]
	ORDER BY u.[url], MIN(r.datecreated)