using System;
using System.Collections.Generic;

namespace Query
{
    public static class Logs
    {
        public static void LogUrl(string url, long? ipaddress = null, int? countrycode = null, float? latitude = null, float? longitude = null)
        {
            Sql.ExecuteNonQuery("Log_Url", new { url, ipaddress, countrycode, latitude, longitude });
        }

        public enum TimeScale
        {
            Hour = 0,
            Day = 1, 
            Week = 2,
            Month = 3,
            Year = 4
        }

        public static List<Models.Logs.UrlAnalytic>GetUrlAnalytics(TimeScale timeScale = TimeScale.Day, DateTime? startDate = null)
        {
            return Sql.Populate<Models.Logs.UrlAnalytic>("Log_GetUrlAnalytics", new { timeScale, startDate });
        }
    }
}
