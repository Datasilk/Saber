using System.Collections.Generic;

namespace Query
{
    public static class PublicApis
    {
        public static List<Models.PublicApi> GetList()
        {
            return Sql.Populate<Models.PublicApi>("PublicApis_GetList");
        }

        public static void Update(string api, bool enabled)
        {
            Sql.ExecuteNonQuery("PublicApi_Update", new { api, enabled });
        }
    }
}
