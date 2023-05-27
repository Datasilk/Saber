using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Saber.Core;
using Saber.Core.Extensions.Strings;

namespace Saber.Services
{
    public class PageResources: Service
    {
        public string Render(string path, string filetypes = "", int sort = 0)
        {
            if (IsPublicApiRequest || !CheckSecurity()) { return AccessDenied(); }
            if(path == "") { return Error("No path specified"); }
            var canEdit = CheckSecurity("code-editor");
            var canUpload = CheckSecurity("upload");
            var view = new View("/Views/PageResources/resources.html");
            var item = new View("/Views/PageResources/resource-item.html");
            var paths = PageInfo.GetRelativePath(path);
            paths[paths.Length - 1] = paths[paths.Length - 1].Split('.', 2)[0];
            var dir = "/" + string.Join("/", paths).ToLower() + "/";
            var pubdir = dir; //published directory
            var noResources = true;

            if (paths[0].ToLower() == "content" && paths[1] == "pages")
            {
                //loading resources for specific page
                pubdir = "/wwwroot" + dir;
                view.Show("for-page");
                view["for-type"] = "Page";
            }
            else
            {
                view.Show("for-folder");
                view["for-type"] = "Website";
                view["folder-path"] = dir.Replace("/wwwroot", "");
            }

            if (Directory.Exists(App.MapPath(pubdir)))
            {
                //first, get list of folders in directory
                var info = new DirectoryInfo(App.MapPath(pubdir));
                var subfolders = info.GetDirectories();
                var html = new StringBuilder();
                var exclude = new List<string>()
                {
                    "wwwroot/editor",
                    "wwwroot/css",
                    "wwwroot/js",
                    "/_thumbs"
                };

                if(path.Replace("content/pages/", "").Split("/").Length > 1)
                {
                    //add parent folder
                    item.Clear();
                    item["file-type"] = "folder";
                    item["file-type"] = "folder";
                    item["file-id"] = "previous-folder";
                    item.Show("svg");
                    item["icon"] = "folder";
                    item["filename"] = "..";
                    html.Append(item.Render());
                    noResources = false;
                }

                foreach(var folder in subfolders)
                {
                    var folderPath = folder.FullName.Replace("\\", "/");
                    var folderName = folder.Name;
                    if (exclude.Any(a => folderPath.Contains(a))) { continue; }
                    item.Clear();
                    item["file-type"] = "folder";
                    item["file-id"] = folderName.Replace(" ", "_");
                    item.Show("svg");
                    item["icon"] = "folder";
                    item["filename"] = folderName;
                    html.Append(item.Render());
                    noResources = false;
                }

                //get list of files in directory (excluding thumbnail images)
                var files = info.GetFiles();
                if(filetypes != "")
                {
                    var ftypes = filetypes;
                    switch (filetypes) {
                        case "images":
                            ftypes = string.Join(", ", Image.Extensions);
                            break;
                    }
                    var types = ftypes.Split(',', StringSplitOptions.TrimEntries);
                    files = files.Where(a => types.Any(b => b == a.Extension)).ToArray();
                }

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
                if (files.Count() > 0)
                {
                    exclude = new List<string>() 
                    { 
                        "web.config", 
                        "web-icon.png"
                    };
                    foreach (var f in files)
                    {
                        if (exclude.Contains(f.Name.ToLower())) { continue; }
                        item.Clear();
                        var ext = f.Name.GetFileExtension();
                        var type = "file";
                        var icon = "";
                        item["img-src-full"] = pubdir.Replace("/wwwroot", "") + f.Name;
                        item["img-src-rel"] = pubdir.Replace("/wwwroot", "") + f.Name;
                        item.Show("check");
                        item.Show("menu");


                        switch (ext.ToLower())
                        {
                            //excluded
                            case "js":
                            case "html":
                            case "css":
                            case "less":
                            case "config":
                                continue;
                            
                            //images
                            
                            case "png":
                            case "jpg":
                            case "jpeg":
                            case "gif":
                            case "webp":
                            case "pbm":
                            case "tiff":
                            case "tga":
                                item.Show("img");
                                item["img-src"] = pubdir.Replace("/wwwroot", "") + Settings.ThumbDir + f.Name;
                                item["img-alt"] = type + " " + f.Name;
                                item.Show("menu-full");
                                item.Show("menu-copy");
                                break;

                            //video 
                            case "3g2":
                            case "3gp":
                            case "3gp2":
                            case "3gpp":
                            case "amv":
                            case "asf":
                            case "avi":
                            case "divx":
                            case "drc":
                            case "dv":
                            case "f4v":
                            case "flv":
                            case "gvi":
                            case "gfx":
                            case "m1v":
                            case "m2v":
                            case "m2t":
                            case "m2ts":
                            case "m4v":
                            case "mkv":
                            case "mov":
                            case "mp2":
                            case "mp2v":
                            case "mp4":
                            case "mp4v":
                            case "mpe":
                            case "mpeg":
                            case "mpeg1":
                            case "mpeg2":
                            case "mpeg4":
                            case "mpg":
                            case "mpv2":
                            case "mts":
                            case "mtv":
                            case "mxf":
                            case "mxg":
                            case "nsv":
                            case "nuv":
                            case "ogg":
                            case "ogm":
                            case "ogv":
                            case "ogx":
                            case "px":
                            case "rec":
                            case "rm":
                            case "rmvb":
                            case "rpl":
                            case "thp":
                            case "tod":
                            case "ts":
                            case "tts":
                            case "tsd":
                            case "vob":
                            case "vro":
                            case "webm":
                            case "wm":
                            case "wmv":
                            case "wtv":
                            case "xesc":
                                icon = "video";
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

                            //case "css":
                            //    icon = "css";
                            //    break;
                            //case "html":
                            //    icon = "html";
                            //    break;
                            //
                            //case "js":
                            //    icon = "js";
                            //    break;
                            //
                            //case "less":
                            //    icon = "less";
                            //    break;

                            case "pdf":
                                icon = "pdf";
                                break;

                            //compressed files
                            case "7z":
                            case "ace":
                            case "arj":
                            case "bz2":
                            case "cab":
                            case "gzip":
                            case "jar":
                            case "lzh":
                            case "rar":
                            case "tar":
                            case "uue":
                            case "xz":
                            case "z":
                            case "zip":
                                icon = "zip";
                                break;

                            default:
                                icon = "";
                                break;
                        }
                        item["file-type"] = type;
                        item["file-id"] = f.Name.ReplaceAll("", new string[] {"@#$%^&*()+=|[]{};'\",<>?~"}).ReplaceAll("_", new string[] {"-", "." }).ToLower();
                        item["filename"] = f.Name;
                        if (item["img-src"] == "")
                        {
                            item.Show("svg");
                            item["icon"] = "file" + (icon != "" ? "-" : "") + icon;
                            item.Show("menu-copy");
                        }
                        html.Append(item.Render());
                        noResources = false;
                    }
                }
                view["resources"] = "<ul>" + html.ToString() + "</ul>";
            }

            //no resources
            if (noResources == true)
            {
                view["resources"] = Cache.LoadFile("/Views/PageResources/no-resources.html");
            }

            if (canUpload)
            {
                view.Show("can-upload");
            }

            return view.Render();
        }

        public string Delete(string path, string file)
        {
            if (IsPublicApiRequest || !CheckSecurity()) { return AccessDenied(); }
            try
            {
                var paths = PageInfo.GetRelativePath(path);
                paths[paths.Length - 1] = paths[paths.Length - 1].Split('.', 2)[0];
                var dir = string.Join("/", paths).ToLower() + "/";
                var pubdir = dir; //published directory
                if (paths[0].ToLower() == "content" && paths[1] == "pages")
                {
                    //loading resources for specific page
                    pubdir = "/wwwroot/" + dir;
                }

                //check for special files that cannot be deleted
                var exclude = new string[] { "web.config" };
                if (exclude.Contains(file)) { return Error(); }

                if (Directory.Exists(App.MapPath(pubdir)))
                {
                    if (File.Exists(App.MapPath(pubdir + file)))
                    {
                        //delete file from disk
                        File.Delete(App.MapPath(pubdir + file));
                    }
                    //check for thumbnails
                    var ext = file.GetFileExtension();
                    switch (ext.ToLower())
                    {
                        case "jpg":
                        case "jpeg":
                        case "png":
                            //delete thumbnail, too
                            if (File.Exists(App.MapPath(pubdir + Settings.ThumbDir + file)))
                            {
                                File.Delete(App.MapPath(pubdir + Settings.ThumbDir + file));
                            }
                            break;
                    }
                }
                return Success();
            }
            catch (Exception) { return Error(); }
        }

        public string DeleteAll(string path, List<string> files)
        {
            if (IsPublicApiRequest || !CheckSecurity()) { return AccessDenied(); }
            try
            {
                var paths = PageInfo.GetRelativePath(path);
                paths[paths.Length - 1] = paths[paths.Length - 1].Split('.', 2)[0];
                var dir = string.Join("/", paths).ToLower() + "/";
                var pubdir = dir; //published directory
                if (paths[0].ToLower() == "content" && paths[1] == "pages")
                {
                    //loading resources for specific page
                    pubdir = "/wwwroot/" + dir;
                }

                //check for special files that cannot be deleted
                var exclude = new string[] { "web.config" };
                foreach(var file in files)
                {
                    if (exclude.Contains(file)) { return Error(); }

                    if (Directory.Exists(App.MapPath(pubdir)))
                    {
                        if (File.Exists(App.MapPath(pubdir + file)))
                        {
                            //delete file from disk
                            File.Delete(App.MapPath(pubdir + file));
                        }
                        //check for thumbnails
                        var ext = file.GetFileExtension();
                        switch (ext.ToLower())
                        {
                            case "jpg":
                            case "jpeg":
                            case "png":
                                //delete thumbnail, too
                                if (File.Exists(App.MapPath(pubdir + Settings.ThumbDir + file)))
                                {
                                    File.Delete(App.MapPath(pubdir + Settings.ThumbDir + file));
                                }
                                break;
                        }
                    }
                }
                
                return Success();
            }
            catch (Exception) { return Error(); }
        }
    }
}
