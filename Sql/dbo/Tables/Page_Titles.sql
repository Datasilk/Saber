CREATE TABLE [dbo].[Page_Titles]
( 
    [titleId] INT NOT NULL PRIMARY KEY,
	[title] NVARCHAR(MAX) NOT NULL, 
    [pos] BIT NOT NULL
)
