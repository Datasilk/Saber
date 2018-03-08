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

        public string Dir(string path)
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            var rawpaths = path.Split('/');
            var rid = path.Replace("/", "_").ToLower();
            var pid = rid.Replace("_", "/");
            var html = new StringBuilder();
            if(pid == "/") { pid = ""; }

            //translate root path to relative path
            var paths = GetRelativePath(path);
            if(paths.Length == 0) { return Error(); }
            var rpath = string.Join("/", paths) + "/";

            var item = new Scaffold("/Services/Editor/file.html", S.Server.Scaffold);
            var items = new List<string>();

            if(paths[0] == "" && paths.Length == 1)
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
                if (Directory.Exists(S.Server.MapPath(rpath))){
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
                                case "html": case "css": case "less": case "js":
                                    items.Add(file.Name); break;
                            }
                        }
                    }
                }
            }

            if (rawpaths.Length > 1)
            {
                //add parent directory;
                html.Append(RenderBrowserItem(item, "goback", "..", "folder", string.Join("/", rawpaths.SkipLast(1)).ToLower()));
            }else if (rawpaths[0] == "content" && rawpaths.Length == 1)
            {
                //add parent directory when navigating to special directory
                html.Append(RenderBrowserItem(item, "goback", "..", "folder", "root"));
            }
            else if(rawpaths.Length == 1 && paths[0] == "")
            {
                //add special directories
                html.Append(RenderBrowserItem(item, "content", "Content", "folder", "content"));
            }

            foreach(var i in items)
            {
                //add directories and files
                var icon = "folder";
                if (i.IndexOf(".") > 0)
                {
                    icon =  "file-" + i.Split('.', 2)[1].ToLower();
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
                item.Data["icon"] = "folder";
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
            if(File.Exists(S.Server.MapPath(string.Join("/", paths))))
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
            if (scaffold.HTML == "") { return ""; }

            //load data from user form fields, depending on selected language
            var config = GetPageConfig(path);
            var lang = UserInfo.language;
            var security = config.ContainsKey("security") ? (config["security"] == "1" ? true : false) : false;
            if(security == true && !CheckSecurity()) { return AccessDenied(); }
            var langfile = S.Server.MapPath(relpath.Replace(file, fileparts[0] + "_" + lang + ".json"));

            //load language-specific content for page
            var pagedata = (Dictionary<string, string>)S.Util.Serializer.ReadObject(S.Server.LoadFileFromCache(langfile, true), typeof(Dictionary<string, string>));
            if (pagedata != null)
            {
                foreach (var item in pagedata)
                {
                    scaffold.Data[item.Key] = item.Value;
                }
            }
            return scaffold.Render();
        }

        private Dictionary<string, string> GetPageConfig(string path)
        {
            var paths = GetRelativePath(path);
            var relpath = string.Join("/", paths);
            var file = paths[paths.Length - 1];
            var fileparts = file.Split(".", 2);
            var configfile = S.Server.MapPath(relpath.Replace(file, fileparts[0] + ".json"));
            var config = (Dictionary<string, string>)S.Util.Serializer.ReadObject(S.Server.LoadFileFromCache(configfile, true), typeof(Dictionary<string, string>));
            if(config != null) { return config; }
            return new Dictionary<string, string>();
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
                if(paths[0].ToLower() == "/content/pages")
                {
                    switch (ext)
                    {
                        case "js": case "css": case "less":
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
                            p.OutputDataReceived += ProcessOutputReceived;
                            p.ErrorDataReceived += ProcessErrorReceived;
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

        private void ProcessOutputReceived(object sender, DataReceivedEventArgs e)
        {
            Process p = sender as Process;
            if (p == null) { return; }
            Console.WriteLine(e.Data);
        }

        private void ProcessErrorReceived(object sender, DataReceivedEventArgs e)
        {
            Process p = sender as Process;
            if (p == null) { return; }
            Console.WriteLine(e.Data);
        }

        public string SaveForm(string path, Dictionary<string, string> fields)
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            var paths = GetRelativePath(path);
            if (paths.Length == 0) { return Error(); }
            try
            {
                //save fields as json
                var last = paths.Length - 1;
                var file = paths[last];
                var fileparts = file.Split(".", 2);
                fileparts[0] += "_fields";
                paths[last] = string.Join(".", fileparts);
                S.Util.Serializer.WriteObjectToFile(fields, S.Server.MapPath(string.Join("/", paths)));
            }
            catch (Exception)
            {
                return Error();
            }

            return Success();
        }
    }
}
