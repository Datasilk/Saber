CREATE PROCEDURE [dbo].[DataSet_GetRecords]
	@datasetId int,
	@start int = 1,
	@length int = 50,
	@lang nvarchar(MAX) = '',
	@search nvarchar(MAX) = '',
	@searchtype int = 0, -- 0 = LIKE %x%, 1 = LIKE x% (starts with), 2 = LIKE %x (ends with), -1 = exact match
	@orderby nvarchar(MAX) = ''
AS
	DECLARE @tableName nvarchar(64)
	SELECT @tableName=tableName FROM DataSets WHERE datasetId=@datasetId

	DECLARE @sql nvarchar(MAX) = 'SELECT * FROM DataSet_' + @tableName + ' WHERE lang=''' + @lang + ''''
	IF @search IS NOT NULL AND @search != '' BEGIN
		--get table columns
		SELECT c.[name] AS col
		INTO #cols 
		FROM sys.columns c
		INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
		WHERE c.object_id = OBJECT_ID('DataSet_' + @tableName)
		AND t.[Name] LIKE '%varchar%'

		SET @sql += ' AND ('
		DECLARE @cursor1 CURSOR, @column nvarchar(32)
		SET @cursor1 = CURSOR FOR 
		SELECT * FROM #cols
		FETCH FROM @cursor1 INTO @column
		WHILE @@FETCH_STATUS = 0 BEGIN
			SET @sql += '[' + @column + '] ' + 
				CASE WHEN @searchtype >= 0 THEN 'LIKE ''' ELSE @search END +
				CASE WHEN @searchtype = 0 THEN '%' + @search + '%' END +
				CASE WHEN @searchtype = 1 THEN @search + '%' END +
				CASE WHEN @searchtype = 2 THEN '%' + @search END +
				CASE WHEN @searchtype >= 0 THEN '''' END
			FETCH FROM @cursor1 INTO @column
			IF @@FETCH_STATUS = 0 BEGIN
				SET @sql = @sql + ' OR '
			END
		END
		CLOSE @cursor1
		DEALLOCATE @cursor1
		SET @sql += ')'
	END

	-- include orderby clause
	IF @orderby IS NOT NULL AND @orderby != '' BEGIN
		SET @sql = @sql + ' ORDERBY ' + @orderby
	END

	--execute generated SQL code
	EXECUTE sp_executesql @sql
	
