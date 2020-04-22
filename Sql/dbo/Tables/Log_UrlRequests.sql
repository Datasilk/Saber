CREATE TABLE [dbo].[Log_UrlRequests]
(
	[urlId] INT NOT NULL , 
    [ipaddress] BIGINT NOT NULL DEFAULT 0, 
    [countrycode] INT NULL, 
    [latitude] FLOAT NULL, --latitude
    [longitude] FLOAT NULL,  --longitude
    [datecreated] DATETIME2 NOT NULL DEFAULT GETUTCDATE(), 
    PRIMARY KEY ([urlId], [ipaddress], [datecreated])
)
