using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Saber.Common.Platform;
using Saber.Common.Extensions.Strings;

namespace Saber.Services
{
    public class ContentFields : Service
    {
        public string Render(string path, string language)
        {
            var paths = PageInfo.GetRelativePath(path);
            var fields = Common.Platform.ContentFields.GetPageContent(path, User.language);
            var html = new StringBuilder();
            var view = new View(string.Join("/", paths) + ".html");
            var fieldText = new View("/Views/ContentFields/text.html");
            foreach (var elem in view.Elements)
            {
                if (elem.Name != "")
                {
                    var val = "";
                    if (fields.ContainsKey(elem.Name))
                    {
                        //get existing content for field
                        val = fields[elem.Name];
                    }

                    //load text field
                    fieldText["title"] = elem.Name.Capitalize().Replace("-", " ").Replace("_", " ");
                    fieldText["id"] = "field_" + elem.Name.Replace("-", "").Replace("_", "");
                    fieldText["default"] = val;
                    html.Append(fieldText.Render());
                }
            }
            if (html.Length == 0)
            {
                var nofields = new View("/Views/ContentFields/nofields.html");
                nofields["filename"] = paths[paths.Length - 1];
                return nofields.Render();
            }
            return html.ToString();
        }

        public string Save(string path, string language, Dictionary<string, string> fields)
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            var data = new Dictionary<string, string>();
            var paths = PageInfo.GetRelativePath(path);
            if (paths.Length == 0) { return Error(); }
            var view = new View(string.Join("/", paths));
            foreach (var elem in view.Elements)
            {
                if (elem.Name != "")
                {
                    var name = elem.Name.Replace("-", "").Replace("_", "");
                    if (fields.ContainsKey(name))
                    {
                        if (fields[name] != "")
                        {
                            data.Add(elem.Name, fields[name]);
                        }
                    }
                }
            }

            try
            {
                //save fields as json
                var json = JsonSerializer.Serialize(data);
                File.WriteAllText(Server.MapPath(Common.Platform.ContentFields.ContentFile(path, language)), json);
            }
            catch (Exception)
            {
                return Error();
            }

            return Success();
        }
    }
}
