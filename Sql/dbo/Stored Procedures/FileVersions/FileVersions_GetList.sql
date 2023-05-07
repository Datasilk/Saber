CREATE PROCEDURE [dbo].[FileVersions_GetList]
AS
	SELECT * FROM FileVersions ORDER BY [file] ASC
