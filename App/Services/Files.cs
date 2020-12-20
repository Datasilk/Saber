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
        private class DirItem
        {
            public bool isDir { get; set; }
            public string Label { get; set; }
            public string Path { get; set; }
            public string Filename { get; set; }
            public string Extension { get; set; }
        }
        public string Dir(string path, string fileTypes = "", bool showDelete = true)
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
            var items = new List<DirItem>();
            var exclude = new string[] { };
            var editable = new string[] { ".js", ".less", ".css" };
            var canEdit = CheckSecurity("code-editor");
            var canDeleteFiles = showDelete == false ? false : CheckSecurity("delete-files");
            var canDeletePages = showDelete == false ? false : CheckSecurity("delete-pages");
            var extensions = new string[] { };
            if (!string.IsNullOrEmpty(fileTypes))
            {
                extensions = fileTypes.Split(",", StringSplitOptions.TrimEntries);
            }

            if (paths[0] == "" && paths.Length == 1 && canEdit)
            {
                //display root folders for website
            }
            else
            {
                //get folder structure from hard drive
                if (Directory.Exists(App.MapPath(rpath)))
                {
                    //get list of directories
                    var info = new DirectoryInfo(App.MapPath(rpath));
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
                            items.Add(new DirItem() { Label = dir.Name, Path = dir.Name, isDir = true });
                        }
                    }

                    //get list of files
                    exclude = new string[] { "gulpfile.js" };
                    if (paths[0] == "/wwwroot" && paths.Length > 1 && paths[1] == "css")
                    {
                        exclude = new string[] { "website.css" };
                    }
                    foreach (var file in info.GetFiles())
                    {
                        if (!exclude.Contains(file.Name.ToLower()) && (extensions.Length == 0 || extensions.Contains(file.Extension))) 
                        {
                            var f = file.Name.GetFileExtension().ToLower();
                            if(paths.Length > 1 && paths[1] == "pages" && f != "html") { continue; }
                            switch (f)
                            {
                                case "html":
                                case "css":
                                case "less":
                                case "js":
                                case "zip":
                                    if(canEdit == false && editable.Any(a => f == a)) { break; }
                                    items.Add(new DirItem() { Label = file.Name, Path = file.Name, Filename = file.Name, Extension = f }); break;
                            }
                        }
                    }
                }
            }

            if (paths[0] == "/Content")
            {
                if (paths.Length == 2)
                {
                    html.Append(RenderBrowserItem(item, "goback", "..", "folder-back", "root", true));
                }
                else
                {
                    html.Append(RenderBrowserItem(item, "goback", "..", "folder-back", string.Join("/", paths.SkipLast(1).ToArray()), true));
                }
            }
            else if (rawpaths.Length > 1)
            {
                //add parent directory;
                html.Append(RenderBrowserItem(item, "goback", "..", "folder-back", string.Join("/", rawpaths.SkipLast(1)), true));
            }
            else if (rawpaths[0] == "content" && rawpaths.Length == 1)
            {
                //add parent directory when navigating to special directory
                html.Append(RenderBrowserItem(item, "goback", "..", "folder-back", "root", true));
            }
            else if (rawpaths.Length == 1 && paths[0] == "")
            {
                //add special directories & files
                html.Append(RenderBrowserItem(item, "wwwroot", "wwwroot", "folder", "wwwroot", true));
                html.Append(RenderBrowserItem(item, "pages", "pages", "folder", "content/pages", true));
                html.Append(RenderBrowserItem(item, "partials", "partials", "folder", "content/partials", true));
                html.Append(RenderBrowserItem(item, "website.less", "website.less", "file", "content/website.less"));
            }
            else if (paths[0] == "/wwwroot")
            {
                html.Append(RenderBrowserItem(item, "goback", "..", "folder-back", "root"));
            }

            foreach (var i in items.OrderBy(a => !a.isDir).ThenBy(a => a.Path))
            {
                //add directories and files
                var icon = "folder";
                if (!i.isDir)
                {
                    icon = "file-" + i.Extension;
                }
                var canDelete = false;
                if(paths.Length > 1 && (paths[1] == "pages" || paths[1] == "partials"))
                {
                    canDelete = canDeletePages;
                }
                else
                {
                    canDelete = canDeleteFiles;
                }

                html.Append(RenderBrowserItem(item, rid + "_" + i.Path.Replace(".", "_").ToLower(), i.Label, icon, (pid != "" ? pid + "/" : "") + i.Path, i.isDir, canDelete));
            }
            return html.ToString();
        }

        private string RenderBrowserItem(View item, string id, string title, string icon, string path, bool isDir = false, bool canDelete = false)
        {
            item["id"] = id;
            item["title"] = title;
            item["icon"] = "folder";
            item["path"] = path;
            item["type"] = isDir ? "folder" : "file";

            if (canDelete) { 
                item.Show("delete");
            }
            if (title.IndexOf(".") > 0)
            {
                item["icon"] = "file-" + title.Split('.')[^1].ToLower();
                if(path.IndexOf("/Content/pages/") == 0)
                {
                    item["onclick"] = "window.parent.location.href='" + path.Replace("/Content/pages", "").Replace(".html", "");
                }
                else
                {
                    item["onclick"] = "S.editor.explorer.open('" + path + "', null, true)";
                }
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

        public string DeleteFile(string path)
        {
            
            if (!CheckSecurity()) { return AccessDenied(); }
            var paths = Core.PageInfo.GetRelativePath(path);
            if(paths[0] == "/pages" || paths[0] == "/partials")
            {
                if (!CheckSecurity("delete-pages"))
                {
                    return AccessDenied();
                }
            }
            else if(!CheckSecurity("delete-files"))
            {
                return AccessDenied();
            }
            try
            {
                File.Delete(App.MapPath(string.Join("/", paths)));
            }
            catch (Exception)
            {
                return Error("Could not delete the selected file");
            }
            return Success();
        }

        public string DeleteFolder(string path)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            var paths = Core.PageInfo.GetRelativePath(path);
            if (paths[0] == "/pages" || paths[0] == "/partials")
            {
                if (paths.Length > 1 && !CheckSecurity("delete-pages"))
                {
                    return AccessDenied();
                }else if(paths.Length == 1)
                {
                    return Error("You are not allowed to delete the folder " + paths[0]);
                }
            }
            else if (!CheckSecurity("delete-files"))
            {
                return AccessDenied();
            }
            try
            {
                Directory.Delete(App.MapPath(string.Join("/", paths)), true);
            }
            catch (Exception)
            {
                return Error("Could not delete the selected folder");
            }
            return Success();
        }
    }
}
