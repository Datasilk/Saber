CREATE TABLE [dbo].[Log_ForgotPasswords]
(
	[logId] INT IDENTITY(1,1) PRIMARY KEY, 
    [email] NVARCHAR(64) NOT NULL, 
    [datecreated] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
)
