using System.Collections.Generic;
using System.Linq;

namespace Query
{
    public static class FileVersions
    {
        public static void Update(string file, int version)
        {
            Sql.ExecuteNonQuery("FileVersion_Update", new { file, version });
        }

        public static Dictionary<string, int> GetList()
        {
            return Sql.Populate<Models.FileVersion>("FileVersions_GetList").ToDictionary(a => a.File, b => b.Version);
        }
    }
}
