using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Net;
using System.Diagnostics;

namespace Saber.Services
{
    public class Editor : Service
    {
        public Editor(Core DatasilkCore) : base(DatasilkCore)
        {
        }

        #region "Utility"

        private string[] GetRelativePath(string path)
        {
            var paths = path.Split('/');

            //translate root path to relative path
            switch (paths[0].ToLower())
            {
                case "root": paths[0] = ""; break;
                case "css": paths[0] = "/CSS"; break;
                case "pages": paths[0] = "/Pages"; break;
                case "partials": paths[0] = "/Partials"; break;
                case "scripts": paths[0] = "/Scripts"; break;
                case "services": paths[0] = "/Services"; break;
                case "content": paths[0] = "/Content/pages"; break;
                default: return new string[] { };
            }
            return paths;
        }

        #endregion

        #region "File System"

        public string Dir(string path)
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            var rawpaths = path.Split('/');
            var rid = path.Replace("/", "_").ToLower();
            var pid = rid.Replace("_", "/");
            var html = new StringBuilder();
            if (pid == "/") { pid = ""; }

            //translate root path to relative path
            var paths = GetRelativePath(path);
            if (paths.Length == 0) { return Error(); }
            var rpath = string.Join("/", paths) + "/";

            var item = new Scaffold("/Services/Editor/file.html", S.Server.Scaffold);
            var items = new List<string>();

            if (paths[0] == "" && paths.Length == 1)
            {
                //display root folders for website
                items = new List<string>()
                {
                    "CSS", "Pages", "Partials", "Scripts", "Services", "access-denied.html", "layout.html"
                };
            }
            else
            {
                //get folder structure from hard drive
                if (Directory.Exists(S.Server.MapPath(rpath)))
                {
                    var info = new DirectoryInfo(S.Server.MapPath(rpath));
                    foreach (var dir in info.GetDirectories())
                    {
                        if (dir.Name.IndexOfAny(new char[] { '.', '_' }) != 0)
                        {
                            items.Add(dir.Name);
                        }
                    }
                    foreach (var file in info.GetFiles())
                    {
                        var f = file.Name.Split(".", 2);
                        if (f.Length > 1)
                        {
                            switch (f[1].ToLower())
                            {
                                case "html":
                                case "css":
                                case "less":
                                case "js":
                                    items.Add(file.Name); break;
                            }
                        }
                    }
                }
            }

            if (rawpaths.Length > 1)
            {
                //add parent directory;
                html.Append(RenderBrowserItem(item, "goback", "..", "folder-back", string.Join("/", rawpaths.SkipLast(1)).ToLower()));
            }
            else if (rawpaths[0] == "content" && rawpaths.Length == 1)
            {
                //add parent directory when navigating to special directory
                html.Append(RenderBrowserItem(item, "goback", "..", "folder-back", "root"));
            }
            else if (rawpaths.Length == 1 && paths[0] == "")
            {
                //add special directories
                html.Append(RenderBrowserItem(item, "content", "Content", "folder", "content"));
            }

            foreach (var i in items)
            {
                //add directories and files
                var icon = "folder";
                if (i.IndexOf(".") > 0)
                {
                    icon = "file-" + i.Split('.', 2)[1].ToLower();
                }
                html.Append(RenderBrowserItem(item, rid + "_" + i.Replace(".", "_").ToLower(), i, icon, (pid != "" ? pid + "/" : "") + i));
            }
            return html.ToString();
        }

        private string RenderBrowserItem(Scaffold item, string id, string title, string icon, string path)
        {
            item.Data["id"] = id;
            item.Data["title"] = title;
            item.Data["icon"] = "folder";

            if (title.IndexOf(".") > 0)
            {
                item.Data["icon"] = "file-" + title.Split('.', 2)[1].ToLower();
                item.Data["onclick"] = "S.editor.explorer.open('" + path + "')";
            }
            else
            {
                item.Data["icon"] = icon;
                item.Data["onclick"] = "S.editor.explorer.dir('" + path + "')";
            }

            return item.Render();
        }

        public string Open(string path)
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            //translate root path to relative path
            var paths = GetRelativePath(path);
            if (paths.Length == 0) { return Error(); }
            if (File.Exists(S.Server.MapPath(string.Join("/", paths))))
            {
                return WebUtility.HtmlEncode(File.ReadAllText(S.Server.MapPath(string.Join("/", paths))));
            }
            var ext = paths[paths.Length - 1].Split(".")[1].ToLower();
            switch (ext)
            {
                case "html": return WebUtility.HtmlEncode("<p>Write content using HTML & CSS</p>");
                case "css": return WebUtility.HtmlEncode("body { }");
                case "less": return WebUtility.HtmlEncode("body {\n    p { }\n}");
                case "js": return WebUtility.HtmlEncode("(function(){\n    //do stuff\n})();");
            }
            return "";
        }

        public string SaveFile(string path, string content)
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            var paths = GetRelativePath(path);
            if (paths.Length == 0) { return Error(); }
            try
            {
                var dir = string.Join("/", paths.Take(paths.Length - 1));
                dir = S.Server.MapPath(dir);
                var filepath = S.Server.MapPath(string.Join("/", paths));
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(filepath, content);

                //clean cache related to file
                var file = paths[paths.Length - 1];
                var ext = file.Split('.', 2)[1].ToLower();
                switch (ext)
                {
                    case "html":
                        //remove cached scaffold object
                        S.Server.Scaffold.Remove(path);
                        break;
                }

                //gulp file
                if (paths[0].ToLower() == "/content/pages")
                {
                    switch (ext)
                    {
                        case "js":
                        case "css":
                        case "less":
                            var p = new Process();
                            p.StartInfo = new ProcessStartInfo()
                            {
                                FileName = "cmd.exe",
                                Arguments = "/c gulp file --path \"" + string.Join("/", paths).ToLower().Substring(1) + "\"",
                                WindowStyle = ProcessWindowStyle.Hidden,
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                RedirectStandardError = true,
                                WorkingDirectory = S.Server.MapPath("/").Replace("App\\", ""),
                                Verb = "runas"
                            };
                            p.OutputDataReceived += GulpOutputReceived;
                            p.ErrorDataReceived += GulpErrorReceived;
                            p.Start();
                            break;
                    }
                }
            }
            catch (Exception)
            {
                return Error();
            }

            return Success();
        }

        private void GulpOutputReceived(object sender, DataReceivedEventArgs e)
        {
            Process p = sender as Process;
            if (p == null) { return; }
            Console.WriteLine(e.Data);
        }

        private void GulpErrorReceived(object sender, DataReceivedEventArgs e)
        {
            Process p = sender as Process;
            if (p == null) { return; }
            Console.WriteLine(e.Data);
        }
        #endregion

        #region "Render Page"

        /// <summary>
        /// Renders a page, including language-specific content, and uses page-specific config to check for security & other features
        /// </summary>
        /// <param name="path">relative path to content (e.g. "content/home")</param>
        /// <returns>rendered HTML of the page content (not including any layout, header, or footer)</returns>
        public string RenderPage(string path)
        {
            //translate root path to relative path
            var paths = GetRelativePath(path);
            var relpath = string.Join("/", paths);
            var file = paths[paths.Length - 1];
            var fileparts = file.Split(".", 2);
            if (paths.Length == 0) { return Error(); }
            var scaffold = new Scaffold(relpath, S.Server.Scaffold);
            if (scaffold.elements.Count == 0) { return ""; }

            //load user content from json file, depending on selected language
            var config = GetPageConfig(path);
            var lang = UserInfo.language;
            var security = config.ContainsKey("security") ? (config["security"] == "1" ? true : false) : false;

            //check security
            if (security == true && !CheckSecurity()) { return AccessDenied(); }

            var contentfile = ContentFile(path, lang);
            var data = (Dictionary<string, string>)S.Util.Serializer.ReadObject(S.Server.LoadFileFromCache(contentfile, true), typeof(Dictionary<string, string>));
            if (data != null)
            {
                foreach (var item in data)
                {
                    scaffold.Data[item.Key] = item.Value;
                }
            }

            return scaffold.Render();
        }
        #endregion

        #region "Page Settings"

        private Dictionary<string, string> GetPageConfig(string path)
        {
            var paths = GetRelativePath(path);
            var relpath = string.Join("/", paths);
            var file = paths[paths.Length - 1];
            var fileparts = file.Split(".", 2);
            var configfile = S.Server.MapPath(relpath.Replace(file, fileparts[0] + ".json"));
            var config = (Dictionary<string, string>)S.Util.Serializer.ReadObject(S.Server.LoadFileFromCache(configfile, true), typeof(Dictionary<string, string>));
            if (config != null) { return config; }
            return new Dictionary<string, string>();
        }
        #endregion

        #region "Content Fields"
        private string ContentFile(string path, string language)
        {
            var paths = GetRelativePath(path);
            var relpath = string.Join("/", paths);
            var file = paths[paths.Length - 1];
            var fileparts = file.Split(".", 2);
            return relpath.Replace(file, fileparts[0] + "_" + language + ".json");

        }

        private Dictionary<string, string> GetPageContent(string path, string language)
        {
            var contentfile = S.Server.MapPath(ContentFile(path, language));
            var content = (Dictionary<string, string>)S.Util.Serializer.ReadObject(S.Server.LoadFileFromCache(contentfile, true), typeof(Dictionary<string, string>));
            if (content != null) { return content; }
            return new Dictionary<string, string>();
        }

        public string RenderContentFields(string path, string language)
        {
            var paths = GetRelativePath(path);
            var content = GetPageContent(path, UserInfo.language);
            var html = new StringBuilder();
            var scaffold = new Scaffold(string.Join("/", paths) + ".html", S.Server.Scaffold);
            var fieldText = new Scaffold("/Services/Editor/Fields/text.html", S.Server.Scaffold);
            var fields = new Dictionary<string, string>();
            var contentfile = ContentFile(path, language);
            if (File.Exists(S.Server.MapPath(contentfile)))
            {
                fields = (Dictionary<string, string>)S.Util.Serializer.ReadObject(S.Server.LoadFileFromCache(contentfile), typeof(Dictionary<string, string>));
            }
            foreach (var elem in scaffold.elements)
            {
                if(elem.name != "")
                {
                    var val = "";
                    if (fields.ContainsKey(elem.name))
                    {
                        //get existing content for field
                        val = fields[elem.name];
                    }

                    //load text field
                    fieldText.Data["title"] = S.Util.Str.Capitalize(elem.name.Replace("-", " ").Replace("_", " "));
                    fieldText.Data["id"] = "field_" + elem.name.Replace("-", "").Replace("_", "");
                    fieldText.Data["default"] = val;
                    html.Append(fieldText.Render());
                }
            }
            return html.ToString();
        }

        public string SaveContentFields(string path, string language, Dictionary<string, string> fields)
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            var data = new Dictionary<string, string>();
            var paths = GetRelativePath(path);
            if (paths.Length == 0) { return Error(); }
            var scaffold = new Scaffold(string.Join("/", paths), S.Server.Scaffold);
            foreach (var elem in scaffold.elements)
            {
                if (elem.name != "")
                {
                    var name = elem.name.Replace("-", "").Replace("_", "");
                    if (fields.ContainsKey(name))
                    {
                        if(fields[name] != "")
                        {
                            data.Add(elem.name, fields[name]);
                        }
                    }
                }
            }

            try
            {
                //save fields as json
                S.Util.Serializer.WriteObjectToFile(data, S.Server.MapPath(ContentFile(path, language)));
            }
            catch (Exception)
            {
                return Error();
            }

            return Success();
        }

        #endregion

        #region "Languages"
        public string Languages()
        {
            var html = new StringBuilder();
            foreach(var lang in S.Server.languages)
            {
                html.Append(lang.Key + ',' + lang.Value + '|');
            }
            return html.ToString().TrimEnd('|');
        }
        #endregion
    }
}
