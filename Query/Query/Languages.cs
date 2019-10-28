using System.Collections.Generic;

namespace Query
{
    public static class Languages
    {
        public static int Create(Models.Language lang)
        {
            return Sql.ExecuteScalar<int>(
                "Language_Create",
                new { lang.langId, lang.language }
            );
        }

    public static void Delete(string langId)
        {
            if(langId == "en") { return;  }
            Sql.ExecuteNonQuery(
                "Language_Delete",
                new { langId }
            );
        }

    public static List<Models.Language> GetList()
        {
            return Sql.Populate<Models.Language>("Languages_GetList");
        }
    }
}
