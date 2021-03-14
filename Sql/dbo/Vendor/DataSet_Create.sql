CREATE PROCEDURE [dbo].[DataSet_Create]
	@userId int NULL = NULL,
	@label nvarchar(64),
	@description nvarchar(MAX),
	@partialview nvarchar(255),
	@columns XML 
	/* example:	
		<columns>
			<column name="label" datatype="text" maxlength="64"></column>
			<column name="description" datatype="text" maxlength="max"></column>
			<column name="datecreated" datatype="datetime" default="now"></column>
		</columns>
	*/
AS
	IF NOT EXISTS(SELECT * FROM DataSets WHERE [label]=@label) BEGIN
		DECLARE @tablename nvarchar(64) = REPLACE(@label, ' ', '_');

		--first, create a new sequence for the dataset
		DECLARE @sql nvarchar(MAX) = 'CREATE SEQUENCE [dbo].[Sequence_DataSet_' + @tableName + '] AS BIGINT START WITH 1 INCREMENT BY 1 NO CACHE'
		EXECUTE sp_executesql @sql


		--create a new table for this dataset
		SET @sql = 'CREATE TABLE [dbo].[DataSet_' + @tablename + '] (Id INT, lang NVARCHAR(16), userId int NOT NULL DEFAULT 1, '
		DECLARE @sqlVars nvarchar(MAX) = ''
		DECLARE @sqlVals nvarchar(MAX) = ''
		DECLARE @indexes nvarchar(MAX) = ''
		
		DECLARE @hdoc INT
		DECLARE @cols TABLE (
			[name] nvarchar(32),
			datatype varchar(32),
			[maxlength] varchar(32),
			[default] varchar(32)
		)
		EXEC sp_xml_preparedocument @hdoc OUTPUT, @columns;

		/* create new addressbook entries based on email list */
		INSERT INTO @cols
		SELECT x.[name], x.datatype, x.[maxlength], x.[default]
		FROM (
			SELECT * FROM OPENXML( @hdoc, '//column', 2)
			WITH (
				[name] nvarchar(32) '@name',
				datatype nvarchar(32) '@datatype',
				[maxlength] nvarchar(32) '@maxlength',
				[default] nvarchar(32) '@default'
			)
		) AS x
	
		DECLARE @cursor CURSOR 
		DECLARE @name nvarchar(32), @datatype nvarchar(32), @maxlength nvarchar(32), @default nvarchar(32)
		SET @cursor = CURSOR FOR
		SELECT [name], [datatype],[maxlength], [default] FROM @cols
		OPEN @cursor
		FETCH NEXT FROM @cursor INTO @name, @datatype, @maxlength, @default
		WHILE @@FETCH_STATUS = 0 BEGIN
			SET @maxlength = ISNULL(@maxlength, '64')
			IF @maxlength = '0' SET @maxlength = 'MAX'
			IF @datatype = 'text' BEGIN
				SET @sql = @sql + '[' + @name + '] NVARCHAR(' + @maxlength + ') NOT NULL DEFAULT '''''
				IF @maxlength != 'max' BEGIN
					SET @indexes = @indexes + 'CREATE INDEX [IX_DataSet_' + @tableName + '_' + @name + '] ON [dbo].[DataSet_' + @tableName + '] ([' + @name + '])'
				END
			END
			IF @datatype = 'image' BEGIN
				SET @sql = @sql + '[' + @name + '] NVARCHAR(MAX) NOT NULL DEFAULT '''''
			END
			IF @datatype = 'number' BEGIN
				SET @sql = @sql + '[' + @name + '] INT NULL ' + CASE WHEN @default IS NOT NULL AND @default != '' THEN 'DEFAULT ' + @default END
				SET @indexes = @indexes + 'CREATE INDEX [IX_DataSet_' + @tableName + '_' + @name + '] ON [dbo].[DataSet_' + @tableName + '] ([' + @name + '])'
			END
			IF @datatype = 'decimal' BEGIN
				SET @sql = @sql + '[' + @name + '] DECIMAL(18,0) NULL ' + CASE WHEN @default IS NOT NULL AND @default != '' THEN 'DEFAULT ' + @default END
				SET @indexes = @indexes + 'CREATE INDEX [IX_DataSet_' + @tableName + '_' + @name + '] ON [dbo].[DataSet_' + @tableName + '] ([' + @name + '])'
			END
			IF @datatype = 'bit' BEGIN
				SET @sql = @sql + '[' + @name + '] BIT NOT NULL DEFAULT ' + CASE WHEN @default = '1' THEN '1' ELSE '0' END
			END
			IF @datatype = 'datetime' BEGIN
				SET @sql = @sql + '[' + @name + '] DATETIME2(7) ' + CASE WHEN @default = 'now' THEN 'NOT NULL DEFAULT GETUTCDATE()' ELSE 'NULL' END
				SET @indexes = @indexes + 'CREATE INDEX [IX_DataSet_' + @tableName + '_' + @name + '] ON [dbo].[DataSet_' + @tableName + '] ([' + @name + '])'
			END
			FETCH NEXT FROM @cursor INTO @name, @datatype, @maxlength, @default
			SET @sql = @sql + ', '
		END
		CLOSE @cursor
		DEALLOCATE @cursor

		SET @sql = @sql + 'PRIMARY KEY (Id, lang))'
		PRINT @sql
		PRINT @indexes

		--execute generated SQL code
		EXECUTE sp_executesql @sql
		EXECUTE sp_executesql @indexes

		--finally, record dataset info
		INSERT INTO DataSets (userId, [label], tableName, partialview, [description], datecreated, deleted)
		VALUES (@userId, @label, @tablename, @partialview, @description, GETUTCDATE(), 0)

		SELECT datasetId FROM DataSets WHERE tableName=@tablename
	END
