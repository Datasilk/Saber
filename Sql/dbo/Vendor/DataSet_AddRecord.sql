CREATE PROCEDURE [dbo].[DataSet_AddRecord]
	@datasetId int,
	@userId int = 0,
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
	AND c.[name] NOT IN ('lang', 'userId')

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
	DECLARE @newId nvarchar(MAX) ='DECLARE @newId int = ' + 
		(CASE WHEN @recordId > 0 THEN CONVERT(nvarchar(16), @recordId) 
		ELSE '0; SET @newId = NEXT VALUE FOR Sequence_DataSet_' + @tableName END) + ';'

	DECLARE @sql nvarchar(MAX) = @newId + 'INSERT INTO DataSet_' + @tableName + ' (Id, lang, userId, ',
	@values nvarchar(MAX) = 'VALUES (@newId, ''' + @lang + ''', ' + 
		(CASE WHEN @userId IS NULL THEN 'NULL, ' ELSE CONVERT(nvarchar(16), @userId) + ', ' END),
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
				SET @values += '''' + REPLACE(@value, '''', '''''') + ''''
			END ELSE BEGIN
				SET @values += @value
			END 
			SET @sql += '[' + @name + ']'
		END

		FETCH NEXT FROM @cursor INTO @name, @value
		IF @@FETCH_STATUS = 0 BEGIN
			IF @datatype != '' BEGIN
				SET @sql += ', '
				SET @values += ', '
			END
		END ELSE BEGIN
			SET @sql += ') '
			SET @values += ')'
		END
	END
	CLOSE @cursor
	DEALLOCATE @cursor

	--finally, execute SQL string
	SET @sql += @values
	EXECUTE sp_executesql @sql


