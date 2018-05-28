using System.Collections.Generic;

namespace Saber.Query
{
    public class Languages : global::Query.QuerySql
    {
        public Languages() { }

        public int Create(Models.Language lang)
        {
            return Sql.ExecuteScalar<int>(
                "Language_Create",
                new Dictionary<string, object>()
                {
                    {"langId", lang.langId},
                    {"language", lang.language}
                }
            );
        }

        public void Delete(string langId)
        {
            if(langId == "en") { return;  }
            Sql.ExecuteNonQuery(
                "Language_Delete",
                new Dictionary<string, object>()
                {
                    {"langId", langId}
                }
            );
        }

        public List<Models.Language> GetList()
        {
            return Sql.Populate<Models.Language>("Languages_GetList",
                new Dictionary<string, object>() {}
            );
        }
    }
}
