using System.Collections.Generic;

namespace Query
{
    public static class Security
    {
        public static class Keys
        {
            public static List<Models.SecurityKey> GetByUserId(int userId)
            {
                return Sql.Populate<Models.SecurityKey>("SecurityKeys_GetByUserId", new { userId });
            }

            public static void Create(int groupId, string key, string value, bool isplatform = false)
            {
                Sql.ExecuteNonQuery("SecurityKey_Create", new { groupId, key, value, isplatform });
            }

            public static void Create(int groupId, List<Models.SecurityKey> keys)
            {
                Sql.ExecuteNonQuery("SecurityKeys_BulkCreate", new { groupId, keys = Common.Serializer.ToXmlDocument(keys).OuterXml });
            }
        }
        
        public static class Groups
        {
            public static void Create(string name)
            {
                Sql.ExecuteNonQuery("SecurityGroup_Create", new { name });
            }

            public static List<Models.SecurityGroup> GetList()
            {
                return Sql.Populate<Models.SecurityGroup>("SecurityGroups_GetList");
            }

            public static void Delete(int groupId)
            {
                Sql.ExecuteNonQuery("SecurityGroup_Delete", new { groupId });
            }

            public static bool Exists(string name)
            {
                return Sql.ExecuteScalar<int>("SecurityGroup_Exists", new { name }) == 1;
            }
            public static void Update(int groupId, string name)
            {
                Sql.ExecuteNonQuery("SecurityGroup_Update", new { groupId, name });
            }
        }

        public static class Users
        {
            public static void Add(int groupId, int userId)
            {
                Sql.ExecuteNonQuery("SecurityUser_Add", new { groupId, userId });
            }
        }
    }
}
