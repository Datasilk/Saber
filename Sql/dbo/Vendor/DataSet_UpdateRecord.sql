CREATE PROCEDURE [dbo].[DataSet_UpdateRecord]
	@datasetId int,
	@recordId int = 0,
	@lang nvarchar(16),
	@fields XML 
	/* example:	
		<fields>
			<field name="label"><value>My Label</value></field>
			<field name="description"><value>The short summary of my record.</value></field>
			<field name="datecreated"><value>2/22/1983</value></field>
		</fields>
	*/
AS
SET NOCOUNT ON
	--first, get a list of column names & data types from our target data set table
	DECLARE @tableName nvarchar(64)
	SELECT @tableName=tableName FROM DataSets WHERE datasetId=@datasetId
	
	SELECT c.[name] AS col, t.[Name] AS datatype
    INTO #cols 
	FROM sys.columns c
    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('DataSet_' + @tableName)

	--next, get a list of fields from XML
	DECLARE @hdoc INT
	DECLARE @fieldlist TABLE (
		[name] nvarchar(64),
		[value] nvarchar(MAX)
	)
	EXEC sp_xml_preparedocument @hdoc OUTPUT, @fields;

	INSERT INTO @fieldlist
	SELECT x.[name], x.[value]
	FROM (
		SELECT * FROM OPENXML( @hdoc, '//field', 2)
		WITH (
			[name] nvarchar(32) '@name',
			[value] nvarchar(MAX)
		)
	) AS x

	--build SQL string from XML fields
	DECLARE @sql nvarchar(MAX) = 'UPDATE DataSet_' + @tableName + ' SET ',
	@name nvarchar(64), @value nvarchar(MAX), 
	@cursor CURSOR, @datatype varchar(16)

	SET @cursor = CURSOR FOR
	SELECT [name], [value] FROM @fieldlist
	OPEN @cursor
	FETCH NEXT FROM @cursor INTO @name, @value
	WHILE @@FETCH_STATUS = 0 BEGIN
		--get data type for column
		SET @datatype = ''
		SELECT @datatype = datatype FROM #cols WHERE col=@name
		IF @datatype != '' BEGIN
			IF @datatype = 'varchar' OR @datatype = 'nvarchar' OR @datatype = 'datetime2' BEGIN
				SET @sql += '[' + @name + '] = ''' + REPLACE(@value, '''', '''''') + ''''
			END ELSE BEGIN
				SET @sql += '[' + @name + '] = ' + @value
			END 
		END

		FETCH NEXT FROM @cursor INTO @name, @value
		IF @@FETCH_STATUS = 0 AND @datatype != '' BEGIN
			SET @sql += ', '
		END
	END
	CLOSE @cursor
	DEALLOCATE @cursor

	--finally, execute SQL string
	SET @sql += ' WHERE Id=' + CONVERT(nvarchar(16), @recordId) + ' AND lang=''' + @lang + ''''
	EXECUTE sp_executesql @sql


