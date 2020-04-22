CREATE TABLE [dbo].[Log_Urls]
(
	[urlId] INT NOT NULL PRIMARY KEY, 
    [url] NVARCHAR(255) NOT NULL, 
    [datecreated] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
)

GO

CREATE INDEX [IX_Log_Urls_Url] ON [dbo].[Log_Urls] ([url])

GO

CREATE INDEX [IX_Log_Urls_DateCreated] ON [dbo].[Log_Urls] (datecreated)
