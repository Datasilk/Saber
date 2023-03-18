﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.IO.Compression;
using Saber.Core;
using Saber.Core.Extensions.Strings;
using dotless.Core;
using dotless.Core.Loggers;
using dotless.Core.configuration;


namespace Saber.Common.Platform
{
    public static class Website
    {
        public static void NewFile(string path, string filename)
        {
            //check for valid path & filename
            CheckFilename(path, filename);
            if (path.IndexOf("content") == 0 && path.IndexOf("content/pages/") == 0)
            {
                throw new ServiceErrorException("You cannot create a file in the content/pages folder");
            }
            else if (path.IndexOf("content") == 0 && path.IndexOf("content/temp/") == 0)
            {
                throw new ServiceErrorException("You cannot create a file in the content/temp folder");
            }
            var paths = PageInfo.GetRelativePath(path.ToLower());
            if(paths.Length == 0)
            {
                throw new ServiceErrorException("You cannot create files in the " + path + " folder");
            }
            var dir = string.Join("/", paths) + "/";
            var fileparts = filename.Split('.', 2);

            //create directory for file if none exist
            if (!Directory.Exists(App.MapPath(dir)))
            {
                Directory.CreateDirectory(App.MapPath(dir));
            }
            if (File.Exists(App.MapPath(dir + filename.Replace(" ", ""))))
            {
                throw new ServiceErrorException("The file alrerady exists");
            }

            //create file with dummy content
            var content = "";
            switch (fileparts[1])
            {
                case "js":
                    content = "(function(){\n\n})();";
                    break;
                case "less":
                    content = ".website {\n\n}";
                    break;
                case "html":
                    content = "<p></p>";
                    break;
            }
            try
            {
                File.WriteAllText(App.MapPath(dir + filename.Replace(" ", "")), content);
            }
            catch (Exception)
            {
                throw new ServiceErrorException("Error creating file");
            }
        }

        public static void NewFolder(string path, string folder)
        {
            //check for root & content folders
            var paths = PageInfo.GetRelativePath(path.ToLower());
            //if (path == "root")
            //{
            //    throw new ServiceErrorException("You cannot create a file in the root folder");
            //}
            if (path == "")
            {
                //throw new ServiceErrorException("You cannot create a file in the content folder");
            }

            //check folder characters
            if (!folder.OnlyLettersAndNumbers(new string[] { "-", "_" }))
            {
                throw new ServiceErrorException("Folder must be alpha-numeric and may contain dashes - and underscores _");
            }

            if (paths.Length == 0)
            {
                throw new ServiceErrorException("No path specified");

            }
            var dir = string.Join("/", paths) + "/" + folder.Replace(" ", "");

            if (!Directory.Exists(App.MapPath(dir)))
            {
                try
                {
                    Directory.CreateDirectory(App.MapPath(dir));
                }
                catch (Exception)
                {
                    throw new ServiceErrorException("Error creating new folder");
                }
            }
        }

        public static void RenameFile(string path, string newname)
        {
            var filename = path.GetFilename();
            CheckFilename(path, filename);
            var paths = PageInfo.GetRelativePath(path.ToLower());
            var dir = string.Join("/", paths.SkipLast(1)) + "/";
            if(paths[1] == "website.less")
            {
                throw new ServiceErrorException("Cannot rename website.less");
            }
            File.Move(App.MapPath(dir + filename), App.MapPath(dir + newname));
        }

        public static bool CheckFilename(string path, string filename)
        {
            if (path == "root" || path == "")
            {
                throw new ServiceErrorException("You cannot create a file in the root folder");
            }
            if (filename == "")
            {
                throw new ServiceErrorException("No filename was specified");
            }

            var paths = PageInfo.GetRelativePath(path.ToLower());
            if (paths.Length == 0)
            {
                throw new ServiceErrorException("No path specified");
            }
            if(paths[0].ToLower().Replace("/", "") != "content" && paths[0].ToLower() != "wwwroot")
            {
                throw new ServiceErrorException("Invalid path specified");
            }
            //check for extension
            var fileparts = filename.Split('.', 2);
            if (fileparts.Length < 2) { throw new ServiceErrorException("Your file must include a file extension"); }
            //check filename characters
            if (!filename.OnlyLettersAndNumbers(new string[] { "-", "_", "." }))
            {
                throw new ServiceErrorException("Filename must be alpha-numeric and may contain dashes - and underscores _");
            }
            return true;
        }

        public static void SaveFile(string path, string content)
        {
            //get relative paths for file
            var paths = PageInfo.GetRelativePath(path);
            if (paths.Length == 0)
            {
                throw new ServiceErrorException("No path specified");
            }
            var dir = string.Join("/", paths.Take(paths.Length - 1));  //relative path
            var absdir = App.MapPath(dir);
            var filepath = "/" + string.Join("/", paths); //relative filename & path
            var file = paths[paths.Length - 1]; //filename only
            var ext = file.Split('.', 2)[1].ToLower(); //file extension only
            if(paths[0] == "/Content") { paths[0] = "content"; }

            //create folder for file
            if (!Directory.Exists(absdir))
            {
                try
                {
                    Directory.CreateDirectory(absdir);
                }
                catch (Exception)
                {
                    throw new ServiceErrorException("Error creating folder for file");
                }
            }
            try
            {
                File.WriteAllText(App.MapPath(filepath), content);
            }
            catch (Exception)
            {
                throw new ServiceErrorException("Error writing to file");
            }

            //clean cache related to file
            ViewCache.Remove(filepath);

            //process saved files

            if (dir.ToLower().IndexOf("content/pages") == 0)
            {
                //create public folder in wwwroot
                var pubdir = "/wwwroot/content/pages/" + string.Join("/", paths.Skip(2)).Replace(file, "");
                if (pubdir[pubdir.Length - 1] != '/') { pubdir += "/"; }
                if (!Directory.Exists(App.MapPath(pubdir)))
                {
                    Directory.CreateDirectory(App.MapPath(pubdir));
                }
                switch (ext)
                {
                    case "js": case "css":
                        //copy resource file to public wwwroot folder
                        File.Copy(App.MapPath(filepath), App.MapPath(pubdir + file), true);
                        break;

                    case "less":
                        //compile less file
                        SaveLessFile(content, pubdir + file.Replace(".less", ".css"), dir);
                        break;
                }
            }
            else if(paths[0].ToLower() == "content")
            {
                switch (paths[1].ToLower())
                {
                    case "partials":
                        switch (paths[2].ToLower())
                        {
                            case "header.less": case "footer.less":
                                //compile website.less
                                SaveLessFile(File.ReadAllText(App.MapPath("/Content/website.less")), "/wwwroot/css/website.css", "/Content/");
                                break;
                            default:
                                var pubpath = "/wwwroot/content/partials/" + string.Join('/', paths.Skip(2).ToArray()).Replace(paths[paths.Length - 1], "");
                                if (paths[paths.Length - 1].Length > 5 && paths[paths.Length - 1].Right(5) == ".less")
                                {
                                    //compile less file
                                    
                                    if (!Directory.Exists(App.MapPath(pubpath)))
                                    {
                                        Directory.CreateDirectory(App.MapPath(pubpath));
                                    }
                                    SaveLessFile(content, pubpath + paths[paths.Length - 1].Replace(".less", ".css"), dir);
                                }else if (paths[paths.Length - 1].Length > 3 && paths[paths.Length - 1].Right(3) == ".js")
                                {
                                    File.Copy(App.MapPath(filepath), App.MapPath(pubpath + paths[paths.Length - 1]), true);
                                }
                                break;
                        }
                        break;
                    case "website.js":
                        File.Copy(App.MapPath(filepath), App.MapPath("/wwwroot/js/" + paths[paths.Length - 1]), true);
                        break;
                    case "website.less":
                        SaveLessFile(content, "/wwwroot/css/website.css", "/Content/");
                        break;
                }
            }
        }

        public static void SaveLessFile(string content, string outputFile, string pathLESS)
        {
            if (pathLESS.StartsWith(App.RootPath))
            {
                pathLESS = pathLESS.Substring(App.RootPath.Length);
            }
                
            Directory.SetCurrentDirectory(App.MapPath(pathLESS));
            var file = App.MapPath(outputFile);
            var dir = file.Replace(file.GetFilename(), "");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var css = Less.Parse(content, new dotless.Core.configuration.DotlessConfiguration()
            {
                Logger = typeof (Website.ConsoleLogger)
            });
            
            if (!string.IsNullOrEmpty(content.Trim()))
            {
                if (string.IsNullOrEmpty(css.Trim()))
                { 
                    throw new ServiceErrorException("LESS compile error, check your syntax");
                }
            }
            File.WriteAllText(file, css);
                
            Directory.SetCurrentDirectory(App.MapPath("/"));
        }

        public class ConsoleLogger : Logger
        {
            public ConsoleLogger(dotless.Core.Loggers.LogLevel level) : base(level) { }

            public ConsoleLogger(DotlessConfiguration config) : this(config.LogLevel)
            {

            }

            protected override void Log(string message)
            {
                Console.WriteLine(message);
            }
        }

        public static void CopyTempWebsite()
        {
            if (!File.Exists(App.MapPath("/Content/pages/home.html")))
            {
                Console.WriteLine("Copying template website to live website...");
                //copy default website since none exists yet
                CreateDirectory("/wwwroot/content/");
                CreateDirectory("/wwwroot/content/pages/");
                CreateDirectory("/wwwroot/fonts/");
                CreateDirectory("/wwwroot/images/");
                CreateDirectory("/wwwroot/js/");
                CreateDirectory("/wwwroot/css/");
                CreateDirectory("/Content/pages/");
                CreateDirectory("/Content/partials/");
                CreateDirectory("/Content/emails/");

                //copy all temp folders into wwwroot
                var dir = new DirectoryInfo(App.MapPath("/Content/temp"));
                var exclude = new string[]
                {
                    "/pages",
                    "/partials",
                    "/emails",
                    "/app-css"
                };
                foreach (var d in dir.GetDirectories())
                {
                    var p = d.FullName.Replace("\\", "/");
                    if (!exclude.Any(a => p.Contains(a)))
                    {
                        Utility.FileSystem.CopyDirectoryContents(d.FullName, App.MapPath("/wwwroot/" + d.Name));
                    }
                }

                //copy Content folders
                Utility.FileSystem.CopyDirectoryContents(App.MapPath("/Content/temp/pages/"), App.MapPath("/Content/pages/"), new string[] { ".html", ".js", ".json", ".less" });
                Utility.FileSystem.CopyDirectoryContents(App.MapPath("/Content/temp/partials/"), App.MapPath("/Content/partials/"), new string[] { ".html", ".js", ".json", ".less" });
                Utility.FileSystem.CopyDirectoryContents(App.MapPath("/Content/temp/emails/"), App.MapPath("/Content/emails/"), new string[] { ".html", ".js", ".json", ".less" });

                File.Copy(App.MapPath("/Content/temp/app-css/website.less"), App.MapPath("/Content/website.less"), true);

                //compile website.less
                SaveLessFile(File.ReadAllText(App.MapPath("/Content/website.less")), "/wwwroot/css/website.css", "/Content/");

                //compile all LESS files for all pages & partials
                var contentFolder = App.MapPath("/Content/");
                var files = Utility.FileSystem.GetAllFiles("/Content/", true, "*.less", new string[] { "/temp/", "/emails/", "website.less" });
                foreach (var file in files)
                {
                    var filename = file.GetFilename();
                    var ext = "." + file.GetFileExtension();
                    var cssfile = file.Replace(contentFolder, "").Replace(ext, ".css");
                    var workingDir = file.Replace(App.RootPath, "").Replace(filename, "");
                    SaveLessFile(File.ReadAllText(file), "/wwwroot/content/" + cssfile, workingDir);
                }

                //copy all JavaScript files (and media files) for pages & partials into wwwroot
                files = Utility.FileSystem.GetAllFiles("/Content/", true, "*.*", new string[] { "/temp/", "/emails/" });
                var excluded = new string[] { ".html", ".less", ".css", ".json" };
                foreach(var file in files)
                {
                    var filename = file.GetFilename();
                    var ext = "." + file.GetFileExtension().ToLower();
                    if (excluded.Contains(ext)) { continue; }
                    var folder = "/wwwroot/content/" + file.Replace(contentFolder, "").Replace(filename, "");
                    if (!Directory.Exists(App.MapPath(folder)))
                    {
                        CreateDirectory(folder);
                    }
                    var workingDir = file.Replace(contentFolder, "");
                    File.Copy(file, App.MapPath(folder) + filename, true);
                }
            }
        }

        private static void CreateDirectory(string path)
        {
            Utility.FileSystem.CreateDirectory(path);
        }

        public static byte[] Export(bool includeWebPages = true, bool includeImages = true, bool includeOtherFiles = true, DateTime? lastModified = null)
        {
            //generate zip archive in memory
            var fms = new MemoryStream();
            using (var ms = new MemoryStream())
            {
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    var files = Core.Website.AllFiles();
                    var slash = (App.IsDocker ? "/" : "\\");
                    var root = App.MapPath("/") + slash;
                    var include = new List<string>();
                    if (includeWebPages == true)
                    {
                        include.AddRange(new string[]
                        {
                            slash + "Content" + slash + "pages" + slash,
                            slash + "Content" + slash + "partials" + slash,
                            slash + "Content" + slash + "website.js",
                            slash + "Content" + slash + "website.less",
                            slash + "wwwroot" + slash + "content" + slash + "pages" + slash,
                            slash + "wwwroot" + slash + "content" + slash + "partials" + slash,
                            slash + "wwwroot" + slash + "js" + slash,
                            slash + "wwwroot" + slash + "css" + slash
                        });
                    }
                    foreach (var file in files)
                    {
                        var ext = file.GetFileExtension();
                        if (includeImages == false && Image.Extensions.Contains("." + ext))
                        {
                            //ignore images
                            continue;
                        }else if(includeOtherFiles == false && !include.Any(a => file.Contains(a)))
                        {
                            //ignore other files
                            continue;
                        }
                        //check if file exists
                        if (!File.Exists(file)) { continue; }

                        if(lastModified != null)
                        {
                            var modified = File.GetLastWriteTime(file);
                            if(modified.CompareTo(lastModified) <= 0) 
                            { 
                                //file is too old
                                continue; 
                            }
                        }

                        //add file to zip archive
                        archive.CreateEntryFromFile(file, file.Replace(root, ""), CompressionLevel.Fastest);
                    }
                }
                ms.Position = 0;
                var buffer = new byte[512];
                var bytesRead = 0;
                while ((bytesRead = ms.Read(buffer, 0, buffer.Length)) > 0)
                    fms.Write(buffer, 0, bytesRead);
            }
            return fms.ToArray();
        }

        public static void Import(Stream stream, bool clean = false, string[] protectedFiles = null)
        {
            //read zip archive contents
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
            {
                var contentFiles = new string[] { "json", "html", "less", "js" };
                var buffer = new byte[512];
                var bytesRead = default(int);

                if (clean == true)
                {
                    //clean all existing files before importing new website
                    var allfiles = Core.Website.AllFiles();
                    if(protectedFiles != null && protectedFiles.Length > 0)
                    {
                        allfiles = allfiles.Where(a => !protectedFiles.Contains(a)).ToList();
                    }
                    foreach(var f in allfiles)
                    {
                        try
                        {
                            File.Delete(f);
                        }
                        catch (Exception) { }
                    }
                }

                //copy all files from zip archive into appropriate folders
                foreach (var entry in archive.Entries)
                {
                    if (entry.Name == "") { continue; }
                    var path = entry.FullName.Replace(entry.Name, "").Replace("\\", "/");
                    var paths = path.Split("/", StringSplitOptions.RemoveEmptyEntries);
                    var exts = entry.Name.ToLower().Split(".");
                    var extension = exts[^1];
                    var copyTo = "";
                    var root = paths[0].ToLower();

                    //ignore restricted file extensions (potentially dangerous malicious files)
                    if (Malicious.FileExtensions.Contains(extension)) { continue; }

                    switch (root)
                    {
                        case "wwwroot":
                            if (paths.Length > 1)
                            {
                                switch (paths[1].ToLower())
                                {
                                    case "content":
                                        if (extension != "js" && extension != "css")
                                        {
                                            copyTo = "/" + string.Join("/", paths) + "/";
                                        }
                                        break;
                                    case "editor":
                                        break;

                                    default:
                                        copyTo = "/" + string.Join("/", paths) + "/";
                                        break;
                                }
                            }
                            break;

                        case "content":
                            if (paths.Length > 1)
                            {
                                if (paths[1].ToLower() != "temp")
                                {
                                    //copy any folder found within the Content folder (excluding temp)
                                    copyTo = path;
                                    break;
                                }
                            }
                            else
                            {
                                switch (entry.Name.ToLower())
                                {
                                    case "website.less":
                                    case "website.json":
                                        copyTo = "/Content/";
                                        break;
                                }
                            }
                            break;
                    }

                    if (copyTo != "")
                    {
                        var fullpath = App.MapPath(copyTo + entry.Name);
                        if (protectedFiles != null && protectedFiles.Contains(fullpath)) { 
                            continue; 
                        }
                        Console.WriteLine("copy to: " + copyTo + entry.Name);
                        if (!Directory.Exists(App.MapPath(copyTo)))
                        {
                            Directory.CreateDirectory(App.MapPath(copyTo));
                        }
                        using (var file = entry.Open())
                        {
                            var fms = new MemoryStream();
                            buffer = new byte[512];
                            bytesRead = 0;
                            byte[] bytes;
                            while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0)
                                fms.Write(buffer, 0, bytesRead);
                            bytes = fms.ToArray();

                            File.WriteAllBytes(fullpath, bytes);
                            if (extension == "less")
                            {
                                //compile less file to public wwwroot folder
                                var lesspath = "";
                                switch (root)
                                {
                                    case "content":
                                        if (entry.Name.ToLower() == "website.less")
                                        {
                                            lesspath = "";
                                        }
                                        else
                                        {
                                            lesspath = "/wwwroot/" + path.Replace("Content/", "content/");
                                        }
                                        break;
                                }
                                //if (!string.IsNullOrEmpty(lesspath))
                                //{
                                //    //compile LESS file into CSS
                                //    Console.WriteLine("compiling LESS file: " + App.MapPath(lesspath + entry.Name.Replace(".less", ".css")));
                                //    if (!Directory.Exists(App.MapPath(lesspath)))
                                //    {
                                //        Directory.CreateDirectory(App.MapPath(lesspath));
                                //    }
                                //    var data = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                                //    Core.Website.SaveLessFile(data, lesspath + entry.Name.Replace(".less", ".css"), copyTo);
                                //}

                            }
                            else if (root == "content" && extension == "js")
                            {
                                //copy js file to public wwwroot folder
                                File.WriteAllBytes(App.MapPath("/wwwroot/" + path.Replace("Content/", "content/") + entry.Name), bytes);
                            }
                        }
                    }
                }

                //clear all cache within Saber
                ViewCache.Clear();
                Cache.Store.Clear();

                //finally, recompile website.css
                //Core.Website.SaveLessFile(File.ReadAllText(App.MapPath("/Content/website.less")), "/wwwroot/css/website.css", "/Content");
            }
        }

        public static class Malicious
        {
            /// <summary>
            /// Used to check for malicious files when importing a website from a zip file
            /// </summary>
            public static string[] FileExtensions = new string[]
            {
            "exe",
            "bat",
            "dll",
            "com",
            "cmd",
            "msi",
            "vb",
            "vbs",
            "ws",
            "wsf",
            "scf",
            "scr",
            "pif",
            "application",
            "gadget",
            "msp",
            "hta",
            "cpl",
            "msc",
            "jar",
            "vbe",
            "wsc",
            "wsh",
            "ps1",
            "ps1xml",
            "ps2",
            "ps2xml",
            "psc1",
            "pcs2",
            "msh",
            "msh1",
            "msh2",
            "mshxml",
            "msh1xml",
            "msh2xml",
            "lnk",
            "inf",
            "ini",
            "reg",
            "docm",
            "dotm",
            "xlsm",
            "xltm",
            "xlam",
            "pptm",
            "potm",
            "ppam",
            "ppsm",
            "sldm"
            };
        }

        public static class Settings
        {
            private static JsonSerializerOptions jsonOptions = new JsonSerializerOptions()
            {
                WriteIndented = true
            };

            public static Models.Website.Settings Load()
            {
                if(App.Website == null)
                {
                    var file = App.MapPath("/Content/website.json");
                    if (File.Exists(file))
                    {
                        var json = Cache.LoadFile(file);
                        if(json != null && json != "")
                        {
                            App.Website = JsonSerializer.Deserialize<Models.Website.Settings>(json);
                        }
                    }
                }
                if(App.Website == null) { App.Website = new Models.Website.Settings(); }
                return App.Website;
            }

            public static void Save(Models.Website.Settings settings)
            {
                App.Website = settings;
                var file = App.MapPath("/Content/website.json");
                File.WriteAllText(file, JsonSerializer.Serialize(settings, jsonOptions));
                Cache.Remove(file);
            }
        }

    }
}
