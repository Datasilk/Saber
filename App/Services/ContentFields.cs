using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Saber.Core;

namespace Saber.Services
{
    public class ContentFields : Service
    {
        public string Render(string path, string language, string container, bool showlang = false, Dictionary<string, string> data = null, List<string> exclude = null)
        {
            if (IsPublicApiRequest || !CheckSecurity("edit-content")) { return AccessDenied(); }
            var paths = PageInfo.GetRelativePath(path);
            var fields = data != null && data.Keys.Count > 0 ? data : Core.ContentFields.GetPageContent(path, language);
            var view = new View(string.Join("/", paths) + (path.Contains(".html") ? "" : ".html"));
            var result = Common.Platform.ContentFields.RenderForm(this, "", view, language, container, fields, exclude?.ToArray());
            
            if (string.IsNullOrWhiteSpace(result))
            {
                var nofields = new View("/Views/ContentFields/nofields.html");
                nofields["filename"] = paths[paths.Length - 1];
                return Response(nofields.Render());
            }
            if (showlang == true)
            {
                var viewlang = new View("/Views/ContentFields/showlang.html");
                viewlang["language"] = App.Languages.Where(a => a.Key == language).First().Value;
                result = viewlang.Render() + result;
            }
            return Response(result);
        }

        public string Save(string path, string language, Dictionary<string, string> fields)
        {
            if (IsPublicApiRequest || !CheckSecurity("edit-content")) { return AccessDenied(); }
            var paths = PageInfo.GetRelativePath(path);
            if (paths.Length == 0) { return Error(); }
            if(language == "") { language = "en"; }
            if (paths[1] == "partials")
            {
                var validated = new Dictionary<string, string>();
                ValidateField(string.Join("/", paths), fields, validated);
                try
                {
                    //save fields as json
                    var json = Core.ContentFields.Serialize(validated);
                    File.WriteAllText(App.MapPath(Core.ContentFields.ContentFile(path, language)), json);
                    //reset view cache for page
                    Website.ResetCache(path, language);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    return Error();
                }
            }
            else
            {
                var config = PageInfo.GetPageConfig(path);
                var validated = new Dictionary<string, string>();
                ValidateField(string.Join("/", paths) + ".html", fields, validated);
                ValidateField("/Content/partials/" + config.header, fields, validated);
                ValidateField("/Content/partials/" + config.footer, fields, validated);
                try
                {
                    //save fields as json
                    var json = Core.ContentFields.Serialize(validated);
                    File.WriteAllText(App.MapPath(Core.ContentFields.ContentFile(path, language)), json);
                    //reset view cache for page
                    Website.ResetCache(path, language);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    return Error();
                }
            }

            return Success();
        }

        private void ValidateField(string path, Dictionary<string, string> fields, Dictionary<string, string> results)
        {
            var view = new View(path);
            foreach (var elem in view.Elements)
            {
                if (elem.Name != "")
                {
                    var name = elem.Name
                        .Replace("!", "_1_")
                        .Replace("@", "_2_")
                        .Replace("#", "_3_")
                        .Replace("$", "_4_")
                        .Replace("%", "_5_")
                        .Replace("^", "_6_")
                        .Replace("&", "_7_")
                        .Replace("*", "_8_")
                        .Replace("(", "_9_")
                        .Replace(")", "_0_")
                        .Replace("+", "_p_")
                        .Replace("[", "_a_")
                        .Replace("{", "_b_")
                        .Replace("]", "_c_")
                        .Replace("}", "_d_")
                        .Replace("=", "_e_")
                        .Replace("|", "_f_")
                        .Replace("\\", "_g_")
                        .Replace(";", "_h_")
                        .Replace(":", "_i_")
                        .Replace("'", "_j_")
                        .Replace(",", "_k_")
                        .Replace("<", "_l_")
                        .Replace(".", "_m_")
                        .Replace(">", "_n_")
                        .Replace("/", "_o_")
                        .Replace("?", "_p_")
                        .Replace("\"", "_q_")
                        .Replace("`", "_r_")
                        .Replace(" ", "_s_")
                        .Replace("~", "_t_");
                    if (fields.ContainsKey(name) && fields[name] != "" && !results.ContainsKey(name))
                    {
                        results.Add(elem.Name, fields[name]);
                    }
                }
            }
        }
    }
}
