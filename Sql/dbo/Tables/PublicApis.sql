﻿CREATE TABLE [dbo].[PublicApis]
(
	[api] NVARCHAR(255) NOT NULL PRIMARY KEY, 
    [enabled] BIT NOT NULL DEFAULT 0
)
