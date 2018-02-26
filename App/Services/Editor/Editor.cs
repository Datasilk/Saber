using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Net;

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
                case "css": paths[0] = "/CSS/"; break;
                case "pages": paths[0] = "/Pages/"; break;
                case "partials": paths[0] = "/Partials/"; break;
                case "scripts": paths[0] = "/Scripts/"; break;
                case "services": paths[0] = "/Services/"; break;
                case "content": paths[0] = "/Content/pages/"; break;
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
            return WebUtility.HtmlEncode("<p>Write content using HTML & CSS</p>");
        }
    }
}
