CREATE PROCEDURE [dbo].[Languages_GetList]
AS
	SELECT * FROM Languages ORDER BY langId ASC
