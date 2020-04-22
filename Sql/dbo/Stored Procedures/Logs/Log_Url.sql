CREATE PROCEDURE [dbo].[Log_Url]
	@url nvarchar(255),
	@ipaddress bigint,
	@countrycode int,
	@latitude float,
	@longitude float
AS
	DECLARE @id int
	SELECT @id = urlId FROM Log_Urls WHERE url=@url
	IF @id IS NULL BEGIN
		SET @id = NEXT VALUE FOR SequenceLogUrls
		INSERT INTO Log_Urls (urlId, url) VALUES (@id, @url)
	END

	INSERT INTO Log_UrlRequests (urlId, ipaddress, countrycode, latitude, longitude) 
	VALUES (@id, @ipaddress, @countrycode, @latitude, @longitude)
