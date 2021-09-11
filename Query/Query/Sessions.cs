namespace Query
{
    public static class Sessions
    {
        public static string Get(string key, int expireInMinutes = 20)
        {
            return Sql.ExecuteScalar<string>("Session_Get", new { key, expireInMinutes });
        }

        public static void Set(string key, string value, int expireInMinutes = 20)
        {
            Sql.ExecuteNonQuery("Session_Set", new { key, value, expireInMinutes });
        }
    }
}
