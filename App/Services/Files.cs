using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Net;
using Saber.Common.Platform;
using Saber.Core.Extensions.Strings;

namespace Saber.Services
{
    public class Files : Service
    {
        public string Dir(string path)
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            var rawpaths = path.Split('/');
            var rid = path.Replace("/", "_");
            var pid = path;
            var html = new StringBuilder();
            if (pid == "/") { pid = ""; }

            //translate root path to relative path
            var paths = Core.PageInfo.GetRelativePath(path);
            if (paths.Length == 0) { return Error(); }
            var rpath = string.Join("/", paths) + "/";

            var item = new View("/Views/FileBrowser/file.html");
            var items = new List<KeyValuePair<string, string>>();
            var exclude = new string[] { };
            var editable = new string[] { ".js", ".less", ".css" };
            var canEdit = CheckSecurity("code-editor");

            if (paths[0] == "" && paths.Length == 1 && canEdit)
            {
                //display root folders for website
                items = new List<KeyValuePair<string, string>>()
                {
                    //new KeyValuePair<string, string>("backups", "backups"),
                    new KeyValuePair<string, string>("website.less", "CSS/website.less"),
                };
            }
            else
            {
                //get folder structure from hard drive
                if (Directory.Exists(App.MapPath(rpath)))
                {
                    //get list of directories
                    var info = new DirectoryInfo(App.MapPath(rpath));
                    if (paths[0] == "/CSS" && paths.Length == 1)
                    {
                        exclude = exclude.Concat(new string[] { "tapestry", "themes" }).ToArray();
                    }
                    if (paths[0] == "/wwwroot")
                    {
                        if (paths.Length == 1)
                        {
                            //exclude internal folders
                            exclude = new string[] { "content", "editor", Settings.ThumbDir.Replace("/", "") };
                        }
                        else
                        {
                            exclude = new string[] { Settings.ThumbDir.Replace("/", "") };
                        }
                    }

                    //get list of sub-directories
                    foreach (var dir in info.GetDirectories())
                    {
                        if (dir.Name.IndexOfAny(new char[] { '.', '_' }) != 0 && !exclude.Contains(dir.Name.ToLower()))
                        {
                            items.Add(new KeyValuePair<string, string>(dir.Name, dir.Name));
                        }
                    }

                    //get list of files
                    exclude = new string[] { "gulpfile.js" };
                    if (paths[0] == "/CSS" && paths.Length == 1)
                    {
                        exclude = new string[] { "platform.less" };
                    }
                    if (paths[0] == "/wwwroot" && paths.Length > 1 && paths[1] == "css")
                    {
                        exclude = new string[] { "website.css" };
                    }
                    foreach (var file in info.GetFiles())
                    {
                        if (!exclude.Contains(file.Name.ToLower()))
                        {
                            var f = file.Name.GetFileExtension();
                            switch (f.ToLower())
                            {
                                case "html":
                                case "css":
                                case "less":
                                case "js":
                                case "zip":
                                    if(canEdit == false && editable.Any(a => f.ToLower() == a)) { break; }
                                    items.Add(new KeyValuePair<string, string>(file.Name, file.Name)); break;
                            }
                        }
                    }
                }
            }

            if (paths[0] == "/Content")
            {
                if (paths.Length == 2)
                {
                    html.Append(RenderBrowserItem(item, "goback", "..", "folder-back", "root"));
                }
            }
            else if (rawpaths.Length > 1)
            {
                //add parent directory;
                html.Append(RenderBrowserItem(item, "goback", "..", "folder-back", string.Join("/", rawpaths.SkipLast(1))));
            }
            else if (rawpaths[0] == "content" && rawpaths.Length == 1)
            {
                //add parent directory when navigating to special directory
                html.Append(RenderBrowserItem(item, "goback", "..", "folder-back", "root"));
            }
            else if (rawpaths.Length == 1 && paths[0] == "")
            {
                //add special directories
                html.Append(RenderBrowserItem(item, "wwwroot", "wwwroot", "folder", "wwwroot"));
                html.Append(RenderBrowserItem(item, "pages", "pages", "folder", "content/pages"));
                html.Append(RenderBrowserItem(item, "partials", "partials", "folder", "content/partials"));
            }
            else if (paths[0] == "/wwwroot")
            {
                html.Append(RenderBrowserItem(item, "goback", "..", "folder-back", "root"));
            }

            foreach (var i in items.OrderBy(a => a.Key))
            {
                //add directories and files
                var icon = "folder";
                if (i.Key.IndexOf(".") > 0)
                {
                    icon = "file-" + i.Key.Split('.', 2)[1].ToLower();
                }
                html.Append(RenderBrowserItem(item, rid + "_" + i.Key.Replace(".", "_").ToLower(), i.Key, icon, (pid != "" ? pid + "/" : "") + i.Value));
            }
            return html.ToString();
        }

        private string RenderBrowserItem(View item, string id, string title, string icon, string path)
        {
            item["id"] = id;
            item["title"] = title;
            item["icon"] = "folder";

            if (title.IndexOf(".") > 0)
            {
                item["icon"] = "file-" + title.Split('.', 2)[1].ToLower();
                item["onclick"] = "S.editor.explorer.open('" + path + "', null, true)";
            }
            else
            {
                item["icon"] = icon;
                item["onclick"] = "S.editor.explorer.dir('" + path + "')";
            }

            return item.Render();
        }

        public string Open(string path, bool pageResource = false)
        {
            if (!CheckSecurity("code-editor")) { return AccessDenied(); }

            //translate root path to relative path
            var paths = Core.PageInfo.GetRelativePath(path);
            if (paths.Length == 0) { return Error(); }
            if (File.Exists(App.MapPath(string.Join("/", paths))))
            {
                if (pageResource == false)
                {
                    //save open tab to user's session
                    User.AddOpenTab(path);
                }
                return WebUtility.HtmlEncode(File.ReadAllText(App.MapPath(string.Join("/", paths))));
            }
            else if (pageResource == true)
            {
                //try to load template page content
                var templatePath = string.Join('/', paths.Take(paths.Length - 1).ToArray()) + "/template." + paths[paths.Length - 1].GetFileExtension();
                if (File.Exists(App.MapPath(templatePath)))
                {
                    return WebUtility.HtmlEncode(File.ReadAllText(App.MapPath(templatePath)));
                }
            }
            var ext = paths[paths.Length - 1].Split(".")[1].ToLower();
            switch (ext)
            {
                case "html": return WebUtility.HtmlEncode("<p>Write content using HTML & CSS</p>");
                case "css": return WebUtility.HtmlEncode(".website { }");
                case "less": return WebUtility.HtmlEncode(".website {\n    p { }\n}");
                case "js": return WebUtility.HtmlEncode("(function(){\n    //do stuff\n})();");
            }
            return "";
        }

        public string Close(string path)
        {
            //remove open tab from user's session
            User.RemoveOpenTab(path);
            return Success();
        }

        public string GetOpenedTabs()
        {
            return JsonResponse(User.GetOpenTabs());
        }

        public string SaveFile(string path, string content)
        {
            if (!CheckSecurity("code-editor")) { return AccessDenied(); }

            try
            {
                Website.SaveFile(path, content);
            }
            catch (ServiceErrorException ex)
            {
                return Error(ex.Message);
            }
            catch (ServiceDeniedException)
            {
                return AccessDenied();
            }
            return Success();
        }

        public string NewFile(string path, string filename)
        {
            if (!CheckSecurity("code-editor")) { return AccessDenied(); }

            try
            {
                Website.NewFile(path, filename);
            }
            catch (ServiceErrorException ex)
            {
                return Error(ex.Message);
            }
            catch (ServiceDeniedException)
            {
                return AccessDenied();
            }
            return Success();
        }

        public string NewFolder(string path, string folder)
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            try
            {
                Website.NewFolder(path, folder);
            }
            catch (ServiceErrorException ex)
            {
                return Error(ex.Message);
            }
            catch (ServiceDeniedException)
            {
                return AccessDenied();
            }
            return Success();
        }
    }
}
