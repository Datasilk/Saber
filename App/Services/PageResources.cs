﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Net;
using Saber.Common.Platform;
using Saber.Common.Extensions.Strings;
using Datasilk.Core.Web;

namespace Saber.Services
{
    public class PageResources: Service
    {
        public string Render(string path, int sort = 0)
        {
            if (!CheckSecurity()) { return AccessDenied(); }
            var view = new View("/Views/PageResources/resources.html");
            var item = new View("/Views/PageResources/resource-item.html");
            var paths = PageInfo.GetRelativePath(path);
            paths[paths.Length - 1] = paths[paths.Length - 1].Split('.', 2)[0];
            var dir = string.Join("/", paths).ToLower() + "/";
            var pubdir = dir; //published directory
            if (paths[0].ToLower() == "/content/pages")
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
                if (files.Count() > 0)
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
                                item.Show("img");
                                item["svg"] = "";
                                item["img-src"] = pubdir.Replace("/wwwroot", "") + Settings.ThumbDir + f.Name;
                                item["img-alt"] = type + " " + f.Name;
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
                        item["filename"] = f.Name;
                        if (type == "file")
                        {
                            item["img"] = "";
                            item.Show("svg");
                            item["icon"] = "file" + (icon != "" ? "-" : "") + icon;
                        }
                        html.Append(item.Render());
                    }

                    view["resources"] = "<ul>" + html.ToString() + "</ul>";
                }
            }

            //no resources
            if (!view.ContainsKey("resources"))
            {
                view["resources"] = Server.LoadFileFromCache("/Views/PageResources/no-resources.html");
            }

            return view.Render();
        }

        public string Delete(string path, string file)
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
                            if (File.Exists(Server.MapPath(pubdir + Settings.ThumbDir + file)))
                            {
                                File.Delete(Server.MapPath(pubdir + Settings.ThumbDir + file));
                            }
                            break;
                    }
                }
                return Success();
            }
            catch (Exception) { return Error(); }
        }
    }
}