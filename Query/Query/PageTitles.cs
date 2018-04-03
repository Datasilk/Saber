﻿using System.Collections.Generic;

namespace Saber.Query
{
    public class PageTitles : QuerySql
    {
        public PageTitles(string connectionString) : base(connectionString)
        {
        }

        public int Create(string title, bool isSuffix)
        {
            return Sql.ExecuteScalar<int>(
                "Page_Title_Create",
                new Dictionary<string, object>()
                {
                    {"title", title },
                    {"pos", isSuffix }
                }
            );
        }

        public void Delete(int id)
        {
            Sql.ExecuteNonQuery(
                "Page_Title_Delete",
                new Dictionary<string, object>()
                {
                    {"titleId", id }
                }
            );
        }

        public enum TitleType
        {
            all = -1,
            prefix = 0,
            suffix = 1
        }

        public List<Models.PageTitle>GetList(TitleType suffix)
        {
            return Sql.Populate<Models.PageTitle>(
                "Page_Titles_GetList",
                new Dictionary<string, object>()
                {
                    {"pos", suffix }
                }
            );
        }

        public string Get(int titleId)
        {
            return Sql.ExecuteScalar<string>("Page_Title_Get",
                new Dictionary<string, object>()
                {
                    {"titleId", titleId }
                }
            );
        }
    }
}
