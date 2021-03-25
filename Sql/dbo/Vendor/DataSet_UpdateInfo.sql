CREATE PROCEDURE [dbo].[DataSet_UpdateInfo]
	@datasetId int,
	@userId int NULL,
	@label nvarchar(64),
	@description nvarchar(MAX)
AS
	DECLARE @uid int
	SELECT @uid = userId FROM DataSets WHERE datasetId=@datasetId
	IF @uid IS NOT NULL AND @userId IS NOT NULL SET @userId = @uid -- make sure to keep original dataset owner
	UPDATE DataSets SET userId=@userId, [label]=@label, [description]=@description WHERE datasetId=@datasetId
