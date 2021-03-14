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
            public static List<Models.SecurityKey> GetList(int groupId)
            {
                return Sql.Populate<Models.SecurityKey>("SecurityKeys_GetList", new { groupId });
            }

            public static void Create(int groupId, string key, bool value, bool isplatform = false)
            {
                Sql.ExecuteNonQuery("SecurityKey_Create", new { groupId, key, value, isplatform });
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

            public static List<Models.SecurityGroup> GetListByIds(int[] groupIds)
            {
                var ids = new Models.Xml.Ids()
                {
                    Id = groupIds
                };
                return Sql.Populate<Models.SecurityGroup>("SecurityGroups_GetListByIds", new { ids = Common.Serializer.ToXmlDocument(ids).OuterXml });
            }
        }

        public static class Users
        {
            public static void Add(int groupId, int userId)
            {
                Sql.ExecuteNonQuery("SecurityUser_Add", new { groupId, userId });
            }
            
            public static void Remove(int groupId, int userId)
            {
                Sql.ExecuteNonQuery("SecurityUser_Remove", new { groupId, userId });
            }

            public static List<Models.SecurityGroup> GetGroups(int userId)
            {
                return Sql.Populate<Models.SecurityGroup>("SecurityUser_GetGroups", new { userId });
            }

            public static bool Check(int userId, string key)
            {
                return Sql.ExecuteScalar<int>("SecurityUser_Check", new { userId, key }) == 1;
            }
        }
    }
}
