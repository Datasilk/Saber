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
}
