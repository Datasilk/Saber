using System.Collections.Generic;

namespace Query
{
    public static class PageTitles
    {
        public static int Create(string title, bool isSuffix)
        {
            return Sql.ExecuteScalar<int>(
                "Page_Title_Create",
                new {title, pos = isSuffix }
            );
        }

        public static void Delete(int titleId)
        {
            Sql.ExecuteNonQuery(
                "Page_Title_Delete",
                new {titleId}
            );
        }

        public enum TitleType
        {
            all = -1,
            prefix = 0,
            suffix = 1
        }

        public static List<Models.PageTitle>GetList(TitleType suffix)
        {
            return Sql.Populate<Models.PageTitle>(
                "Page_Titles_GetList",
                new {pos = (int)suffix}
            );
        }

        public static string Get(int titleId)
        {
            return Sql.ExecuteScalar<string>("Page_Title_Get",
                new { titleId }
            );
        }
    }
}
