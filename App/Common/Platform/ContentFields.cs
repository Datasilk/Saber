using System.Collections.Generic;
using System.Text.Json;

namespace Saber.Common.Platform
{
    public class ContentFields
    {
        public static string ContentFile(string path, string language)
        {
            var paths = PageInfo.GetRelativePath(path);
            var relpath = string.Join("/", paths);
            var file = paths[paths.Length - 1];
            var fileparts = file.Split(".", 2);
            return relpath.Replace(file, fileparts[0] + "_" + language + ".json");
        }

        public static Dictionary<string, string> GetPageContent(string path, string language)
        {
            var contentfile = Server.MapPath(ContentFile(path, language));
            var content = JsonSerializer.Deserialize<Dictionary<string, string>>(Server.LoadFileFromCache(contentfile));
            if (content != null) { return content; }
            return new Dictionary<string, string>();
        }
    }
}
