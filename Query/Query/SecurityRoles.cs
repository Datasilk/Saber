using System.Collections.Generic;

namespace Query
{
    public static class SecurityRoles
    {
        public static List<Models.SecurityRole> GetByUserId(int userId)
        {
            return Sql.Populate<Models.SecurityRole>("SecurityRoles_GetByUserId", new { userId });
        }

        public static void Create(int userId, string key, string value, bool isplatform = false)
        {
            Sql.ExecuteNonQuery("SecurityRole_Create", new { userId, key, value, isplatform });
        }
    }
}
