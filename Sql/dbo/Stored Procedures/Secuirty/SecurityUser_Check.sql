CREATE PROCEDURE [dbo].[SecurityUser_Check]
	@userId int,
	@key varchar(32)
AS
	SELECT CASE WHEN EXISTS(
		SELECT * FROM Security_Keys k
		JOIN Security_Users u ON u.groupId = k.groupId
		WHERE [key]=@key
		AND u.userId=@userId
	) THEN 1 ELSE 0 END
