﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.Http;
using Utility.Serialization;
using Utility.Strings;
using Saber.Common.Platform;

namespace Saber.Services
{
    public class Editor : Service
    {

        private string thumbdir = "_thumbs/"; 

        public Editor(HttpContext context) : base(context)
        {
        }

        #region "File System"

        public string Dir(string path)
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            var rawpaths = path.Split('/');
            var rid = path.Replace("/", "_");
            var pid = rid.Replace("_", "/");
            var html = new StringBuilder();
            if (pid == "/") { pid = ""; }

            //translate root path to relative path
            var paths = PageInfo.GetRelativePath(path);
            if (paths.Length == 0) { return Error(); }
            var rpath = string.Join("/", paths) + "/";

            var item = new Scaffold("/Views/FileBrowser/file.html", Server.Scaffold);
            var items = new List<string>();
            var exclude = new string[] { };

            if (paths[0] == "" && paths.Length == 1)
            {
                //display root folders for website
                items = new List<string>()
                {
                    "CSS", "Scripts"
                };
            }
            else if (paths[0] == "/wwwroot")
            {
                //get folder structure for resources (wwwroot) from hard drive
                if (Directory.Exists(Server.MapPath(rpath)))
                {
                    var info = new DirectoryInfo(Server.MapPath(rpath));
                    if (paths.Length == 1) {
                        //exclude internal folders
                        exclude = new string[] { "content", "css", "editor", "js", "themes", thumbdir.Replace("/", "") };
                    }
                    else
                    {
                        exclude = new string[] { thumbdir.Replace("/", "") };
                    }
                    foreach (var dir in info.GetDirectories())
                    {
                        if (!exclude.Contains(dir.Name.ToLower()))
                        {
                            items.Add(dir.Name);
                        }
                    }
                }
            }
            else
            {
                //get folder structure from hard drive
                if (Directory.Exists(Server.MapPath(rpath)))
                {
                    //get list of directories
                    var info = new DirectoryInfo(Server.MapPath(rpath));
                    if (paths[0] == "/CSS" && paths.Length == 1)
                    {
                        exclude = exclude.Concat(new string[]{ "tapestry", "themes" }).ToArray();
                    }
                    else if (paths[0] == "/Scripts")
                    {
                        exclude = exclude.Concat(new string[] { "min-maps", "platform", "selector", "utility" }).ToArray();
                    }

                    foreach (var dir in info.GetDirectories())
                    {
                        if (dir.Name.IndexOfAny(new char[] { '.', '_' }) != 0 && !exclude.Contains(dir.Name.ToLower()))
                        {
                            items.Add(dir.Name);
                        }
                    }

                    //get list of files
                    exclude = new string[] { "gulpfile.js" };
                    if (paths[0] == "/CSS" && paths.Length == 1)
                    {
                        exclude = new string[] { "platform.less" };
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
                                    items.Add(file.Name); break;
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
                //html.Append(RenderBrowserItem(item, "content", "Content", "folder", "content"));
                html.Append(RenderBrowserItem(item, "wwwroot", "wwwroot", "folder", "wwwroot"));
                html.Append(RenderBrowserItem(item, "partials", "partials", "folder", "content/partials"));
            }
            else if (paths[0] == "/wwwroot")
            {
                html.Append(RenderBrowserItem(item, "goback", "..", "folder-back", "root"));
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
            var paths = PageInfo.GetRelativePath(path);
            if (paths.Length == 0) { return Error(); }
            if (File.Exists(Server.MapPath(string.Join("/", paths))))
            {
                return WebUtility.HtmlEncode(File.ReadAllText(Server.MapPath(string.Join("/", paths))));
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
            if (!CheckSecurity()) { return AccessDenied(); }

            try
            {
                Website.NewFile(path, filename);
            }
            catch (ServiceErrorException ex) {
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


        #endregion

        #region "Render Page"

        /// <summary>
        /// Renders a page, including language-specific content, and uses page-specific config to check for security & other features
        /// </summary>
        /// <param name="path">relative path to content (e.g. "content/home")</param>
        /// <returns>rendered HTML of the page content (not including any layout, header, or footer)</returns>
        public string RenderPage(string path)
        {
            try
            {
                return Render.Page(path, this);
            }catch(ServiceErrorException ex)
            {
                return Error(ex.Message);
            }catch(ServiceDeniedException)
            {
                return AccessDenied();
            }
        }
        #endregion

        #region "Content Fields"
        public string RenderContentFields(string path, string language)
        {
            var paths = PageInfo.GetRelativePath(path);
            var content = ContentFields.GetPageContent(path, User.language);
            var html = new StringBuilder();
            var scaffold = new Scaffold(string.Join("/", paths) + ".html", Server.Scaffold);
            var fieldText = new Scaffold("/Views/ContentFields/text.html", Server.Scaffold);
            var fields = new Dictionary<string, string>();
            var contentfile = ContentFields.ContentFile(path, language);
            if (File.Exists(Server.MapPath(contentfile)))
            {
                fields = (Dictionary<string, string>)Serializer.ReadObject(Server.LoadFileFromCache(contentfile), typeof(Dictionary<string, string>));
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
                    fieldText.Data["title"] = elem.name.Capitalize().Replace("-", " ").Replace("_", " ");
                    fieldText.Data["id"] = "field_" + elem.name.Replace("-", "").Replace("_", "");
                    fieldText.Data["default"] = val;
                    html.Append(fieldText.Render());
                }
            }
            if(html.Length == 0)
            {
                var nofields = new Scaffold("/Views/ContentFields/nofields.html", Server.Scaffold);
                nofields.Data["filename"] = paths[paths.Length - 1];
                return nofields.Render();
            }
            return html.ToString();
        }

        public string SaveContentFields(string path, string language, Dictionary<string, string> fields)
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            var data = new Dictionary<string, string>();
            var paths = PageInfo.GetRelativePath(path);
            if (paths.Length == 0) { return Error(); }
            var scaffold = new Scaffold(string.Join("/", paths), Server.Scaffold);
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
                Serializer.WriteObjectToFile(data, Server.MapPath(ContentFields.ContentFile(path, language)));
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
            foreach(var lang in Server.languages)
            {
                html.Append(lang.Key + ',' + lang.Value + '|');
            }
            return html.ToString().TrimEnd('|');
        }
        #endregion

        #region "Page Settings"
        public string RenderPageSettings(string path)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            var config = PageInfo.GetPageConfig(path);
            var scaffold = new Scaffold("/Views/PageSettings/settings.html", Server.Scaffold);
            var prefixes = new StringBuilder();
            var suffixes = new StringBuilder();

            //generate list of page prefixes
            var query = new Query.PageTitles();
            var titles = query.GetList(Query.PageTitles.TitleType.all);
            prefixes.Append("<option value=\"0\">[None]</option>\n");
            suffixes.Append("<option value=\"0\">[None]</option>\n");
            foreach (var t in titles)
            {
                if(t.pos == false)
                {
                    prefixes.Append("<option value=\"" + t.titleId + "\"" + (config.title.prefixId == t.titleId ? " selected" : "") + ">" + t.title + "</option>\n");
                }
                else
                {
                    suffixes.Append("<option value=\"" + t.titleId + "\"" + (config.title.suffixId == t.titleId ? " selected" : "") + ">" + t.title + "</option>\n");
                }
            }

            scaffold.Data["page-title"] = config.title.body;
            scaffold.Data["page-title-prefixes"] = prefixes.ToString();
            scaffold.Data["page-title-suffixes"] = suffixes.ToString();
            scaffold.Data["page-description"] = config.description;

            return scaffold.Render();
        }

        public string UpdatePageTitle(string path, int prefixId, int suffixId, string title)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            try
            {
                var config = PageInfo.GetPageConfig(path);
                config.title.body = title;
                config.title.prefixId = prefixId;
                config.title.suffixId = suffixId;
                var query = new Query.PageTitles();
                if (prefixId == 0)
                {
                    config.title.prefix = "";
                }
                else
                {
                    config.title.prefix = query.Get(prefixId);
                }
                if (suffixId == 0)
                {
                    config.title.suffix = "";
                }
                else
                {
                    config.title.suffix = query.Get(suffixId);
                }
                PageInfo.SavePageConfig(path, config);
                return Success();
            }
            catch (Exception)
            {
                return Error();
            }
        }

        /// <summary>
        /// Creates a partial page title, such as the name of the website or the authors name, 
        /// which can be used as a prefix or suffix for the web page title
        /// </summary>
        /// <param name="title"></param>
        /// <param name="prefix">Whether or not the page title part is a prefix (true) or suffix (false)</param>
        /// <returns></returns>
        public string CreatePageTitlePart(string title, bool prefix)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            //add space at end if user didn't
            if(prefix == true)
            {
                if (title.Last() != ' ') { title += " "; }
            }
            else
            {
                if (title.First() != ' ') { title = " " + title; }
            }
            
            try
            {
                var query = new Query.PageTitles();
                var id = query.Create(title, !prefix);
                return id + "|" + title;
            }
            catch (Exception) { return Error(); }
        }

        public string UpdatePageDescription(string path, string description)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            try
            {
                var config = PageInfo.GetPageConfig(path);
                config.description = description;
                var query = new Query.PageTitles();
                PageInfo.SavePageConfig(path, config);
                return Success();
            }
            catch (Exception)
            {
                return Error();
            }
        }
        #endregion

        #region "Page Resrouces"
        public string RenderPageResources(string path, int sort = 0)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            var scaffold = new Scaffold("/Views/PageResources/resources.html", Server.Scaffold);
            var item = new Scaffold("/Views/PageResources/resource-item.html", Server.Scaffold);
            var paths = PageInfo.GetRelativePath(path);
            paths[paths.Length - 1] = paths[paths.Length - 1].Split('.', 2)[0];
            var dir = string.Join("/", paths).ToLower() + "/";
            var pubdir = dir; //published directory
            if(paths[0].ToLower() == "/content/pages")
            {
                //loading resources for specific page
                pubdir = "/wwwroot" + dir;
                scaffold.Data["for-page"] = "1";
                scaffold.Data["for-type"] = "Page";
            }
            else
            {
                scaffold.Data["for-folder"] = "1";
                scaffold.Data["for-type"] = "Website";
                scaffold.Data["folder-path"] = dir.Replace("/wwwroot", "");
            }
            if (Directory.Exists(Server.MapPath(pubdir)))
            {
                //get list of files in directory (excluding thumbnail images)
                var info = new DirectoryInfo(Server.MapPath(pubdir));
                var files = info.GetFiles();

                //sort files
                switch (sort)
                {
                    case 0: //file type
                        files = files.OrderBy(f => f.Extension).ToArray();
                        break;

                    case 1: //alphabetical
                        files = files.OrderBy(f => f.Name).ToArray();
                        break;

                    case 2: //date created asc
                        files = files.OrderBy(f => f.CreationTime).ToArray();
                        break;

                    case 3: //date created desc
                        files = files.OrderBy(f => f.CreationTime).Reverse().ToArray();
                        break;
                }

                //generate HTML list of resources
                if(files.Count() > 0)
                {
                    var html = new StringBuilder();
                    var exclude = new string[] { "web.config" };
                    foreach (var f in files)
                    {
                        if (exclude.Contains(f.Name.ToLower())) { continue; }
                        var ext = f.Name.GetFileExtension();
                        var type = "file";
                        var icon = "";
                        switch (ext.ToLower())
                        {
                            case "png": //images
                            case "jpg":
                            case "jpeg":
                            case "gif":
                                type = "image";
                                item.Data["img"] = "1";
                                item.Data["svg"] = "";
                                item.Data["img-src"] = pubdir.Replace("/wwwroot","") + thumbdir + f.Name;
                                item.Data["img-alt"] = type + " " + f.Name;
                                break;

                            //video 
                            case "3g2":case "3gp":case "3gp2":case "3gpp":case "amv":case "asf":case "avi":case "divx":case "drc":case "dv":case "f4v":case "flv":case "gvi":case "gfx":case "m1v":case "m2v":case "m2t":case "m2ts":
                            case "m4v":case "mkv":case "mov":case "mp2":case "mp2v":case "mp4":case "mp4v":case "mpe":case "mpeg":case "mpeg1":case "mpeg2":case "mpeg4":case "mpg":case "mpv2":case "mts":case "mtv":case "mxf":
                            case "mxg":case "nsv":case "nuv":case "ogg":case "ogm":case "ogv":case "ogx":case "px":case "rec":case "rm":case "rmvb":case "rpl":case "thp":case "tod":case "ts":case "tts":case "tsd":case "vob":
                            case "vro":case "webm":case "wm":case "wmv":case "wtv":case "xesc":
                                icon = "video";
                                break;

                            case "css":
                                icon = "css";
                                break;

                            case "doc": //documents
                            case "docx":
                            case "rtf":
                            case "txt":
                            case "odt":
                            case "uot":
                                icon = "doc";
                                break;

                            case "exe": //executables
                                icon = "exe";
                                break;

                            case "html":
                                icon = "html";
                                break;

                            case "js":
                                icon = "js";
                                break;

                            case "less":
                                icon = "less";
                                break;

                            case "pdf":
                                icon = "pdf";
                                break;

                            //compressed files
                            case "7z": case "ace":case "arj":case "bz2":case "cab":case "gzip":case "jar":case "lzh":case "rar":
                            case "tar":case "uue":case "xz":case "z":case "zip":
                                icon = "zip";
                                break;

                            default:
                                icon = "";
                                break;
                        }
                        item.Data["file-type"] = type;
                        item.Data["filename"] = f.Name;
                        if(type == "file")
                        {
                            item.Data["img"] = "";
                            item.Data["svg"] = "1";
                            item.Data["icon"] = "file" + (icon != "" ? "-" : "") + icon;
                        }
                        html.Append(item.Render());
                    }

                    scaffold.Data["resources"] = "<ul>" + html.ToString() + "</ul>";
                }
            }

            //no resources
            if(!scaffold.Data.ContainsKey("resources")) {
                scaffold.Data["resources"] = Server.LoadFileFromCache("/Views/PageResources/no-resources.html");
            }

            return scaffold.Render();
        }

        public string DeletePageResource(string path, string file)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            try
            {
                var paths = PageInfo.GetRelativePath(path);
                paths[paths.Length - 1] = paths[paths.Length - 1].Split('.', 2)[0];
                var dir = string.Join("/", paths).ToLower() + "/";
                var pubdir = dir; //published directory
                if (paths[0].ToLower() == "/content/pages")
                {
                    //loading resources for specific page
                    pubdir = "/wwwroot" + dir;
                }

                //check for special files that cannot be deleted
                var exclude = new string[] { "web.config" };
                if (exclude.Contains(file)) { return Error(); }

                if (Directory.Exists(Server.MapPath(pubdir)))
                {
                    if (File.Exists(Server.MapPath(pubdir + file)))
                    {
                        //delete file from disk
                        File.Delete(Server.MapPath(pubdir + file));
                    }
                    var ext = file.GetFileExtension();
                    switch (ext.ToLower())
                    {
                        case "jpg":
                        case "jpeg":
                        case "png":
                            //delete thumbnail, too
                            if (File.Exists(Server.MapPath(pubdir + thumbdir + file)))
                            {
                                File.Delete(Server.MapPath(pubdir + thumbdir + file));
                            }
                            break;
                    }
                }
                return Success();
            }
            catch (Exception) { return Error(); }
        }
        #endregion
    }
}
