CREATE PROCEDURE [dbo].[SecurityGroups_GetListByIds]
	@ids XML 
	/* example:	
		<ids>
			<id>1</id>
			<id>3</id>
			<id>4</id>
		</ids>
	*/
AS
	DECLARE @hdoc INT
	DECLARE @idlist TABLE (
		[id] int
	)
	EXEC sp_xml_preparedocument @hdoc OUTPUT, @ids

	/* create new addressbook entries based on email list */
	INSERT INTO @idlist
	SELECT x.[id]
	FROM (
		SELECT CONVERT(INT, CONVERT(NVARCHAR(3), [text])) AS id FROM OPENXML( @hdoc, '//id', 2)
	) AS x

	SELECT * FROM Security_Groups
	WHERE groupId IN (SELECT id FROM @idlist)