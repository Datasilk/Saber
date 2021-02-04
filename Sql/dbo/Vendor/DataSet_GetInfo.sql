CREATE PROCEDURE [dbo].[DataSet_GetInfo]
	@datasetId int
AS
	SELECT * FROM DataSets WHERE datasetId=@datasetId
