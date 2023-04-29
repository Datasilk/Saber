using System;

namespace Query.Models.Logs
{
    public class Url
    {
        public int urlId { get; set; }
        public string url { get; set; }
        public DateTime datecreated { get; set; }
    }

    public class UrlRequest: Url
    {
        public long? ipaddress { get; set; }
        public int? countrycode { get; set; }
        public float? latitude { get; set; }
        public float? longitude { get; set; }
    }

    public class UrlAnalytic: Url
    {
        public long total { get; set; }
        public string datesort { get; set; }
    }

    public class Error
    {
        public int logId { get; set; }
        public DateTime datecreated { get; set; }
        public int userId { get; set; }
        public string url { get; set; }
        public string area { get; set; }
        public string message { get; set; }
        public string stacktrace { get; set; }
    }
}
