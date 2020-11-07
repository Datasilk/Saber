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
            var contentfile = App.MapPath(ContentFile(path, language));
            var json = Cache.LoadFile(contentfile);
            if(json == "") { json = "{}"; }
            var content = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (content != null) { return content; }
            return new Dictionary<string, string>();
        }
    }
}
