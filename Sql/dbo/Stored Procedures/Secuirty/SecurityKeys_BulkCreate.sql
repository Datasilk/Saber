CREATE PROCEDURE [dbo].[SecurityKeys_BulkCreate]
	@groupId int,
	@keys XML 
	/* example:	
		<keys>
			<key name="manage-users" value="1" isplatform="1"></key>
			<key name="manage-security" value="1" isplatform="1"></key>
			<key name="upload" value="0" isplatform="1"></key>
		</keys>
	*/
AS
	
	DECLARE @hdoc INT
	DECLARE @newkeys TABLE (
		[key] varchar(32),
		[value] bit,
		isplatform bit
	)
	EXEC sp_xml_preparedocument @hdoc OUTPUT, @keys;

	/* create new addressbook entries based on email list */
	INSERT INTO @newkeys
	SELECT x.[key], x.[value], x.isplatform
	FROM (
		SELECT * FROM OPENXML( @hdoc, '//key', 2)
		WITH (
			[key] varchar(32) '@name',
			[value] bit '@value',
			isplatform bit '@isplatform'
		)
	) AS x
	
	DECLARE @cursor CURSOR 
	DECLARE @key varchar(32), @value bit, @isplatform bit
	SET @cursor = CURSOR FOR
	SELECT * FROM @newkeys
	FETCH NEXT FROM @cursor INTO @key, @value, @isplatform
	WHILE @@FETCH_STATUS = 0 BEGIN
		IF EXISTS(SELECT * FROM Security_Keys WHERE groupId=@groupId AND [key]=@key) BEGIN
			UPDATE Security_Keys SET [value] = @value WHERE groupId=@groupId AND [key]=@key 
		END ELSE BEGIN
			INSERT INTO Security_Keys (groupId, [key], [value], isplatform) 
			VALUES (@groupId, @key, @value, @isplatform)
		END
		FETCH NEXT FROM @cursor INTO @key, @value, @isplatform
	END
	CLOSE @cursor
	DEALLOCATE @cursor
