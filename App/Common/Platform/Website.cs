using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Saber.Core;
using Saber.Core.Extensions.Strings;
using dotless.Core;


namespace Saber.Common.Platform
{
    public static class Website
    {
        public static void NewFile(string path, string filename)
        {
            //check for root & content folders
            if (path == "root" || path == "")
            {
                throw new ServiceErrorException("You cannot create a file in the root folder");
            }
            if (path.IndexOf("content") == 0 && path.IndexOf("content/pages/") == 0)
            {
                throw new ServiceErrorException("You cannot create a file in the content/pages folder");
            }

            //check filename characters
            if (!filename.OnlyLettersAndNumbers(new string[] { "-", "_", "." }))
            {
                throw new ServiceErrorException("Filename must be alpha-numeric and may contain dashes - and underscores _");
            }

            var paths = PageInfo.GetRelativePath(path.ToLower());
            if (paths.Length == 0)
            {
                throw new ServiceErrorException("No path specified");
            }
            var fileparts = filename.Split('.', 2);
            var dir = string.Join("/", paths) + "/";
            if(fileparts.Length < 2) { throw new ServiceErrorException("Your file must include a file extension"); }

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
            if (path == "root")
            {
                throw new ServiceErrorException("You cannot create a file in the root folder");
            }
            if (paths[0] == "/Content" && paths.Length == 0)
            {
                throw new ServiceErrorException("You cannot create a file in the content folder");
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
            var filepath = string.Join("/", paths); //relative filename & path
            var file = paths[paths.Length - 1]; //filename only
            var ext = file.Split('.', 2)[1].ToLower(); //file extension only

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

            if (paths[0].ToLower() == "/content/pages")
            {
                //create public folder in wwwroot
                var pubdir = "/wwwroot/content/pages/" + string.Join("/", paths.Skip(1)).Replace(file, "");
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
            else if(paths[0].ToLower() == "/content")
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
                                if (paths[2].Right(5) == ".less")
                                {
                                    //compile less file
                                    
                                    if (!Directory.Exists(App.MapPath(pubpath)))
                                    {
                                        Directory.CreateDirectory(App.MapPath(pubpath));
                                    }
                                    SaveLessFile(content, pubpath + paths[paths.Length - 1].Replace(".less", ".css"), dir);
                                }else if (paths[2].Right(3) == ".js")
                                {
                                    File.Copy(App.MapPath(filepath), App.MapPath(pubpath + paths[paths.Length - 1]), true);
                                }
                                break;
                        }
                        break;
                    case "website.less":
                        SaveLessFile(content, "/wwwroot/css/website.css", "/Content/");
                        break;
                }
            }
        }

        public static void SaveLessFile(string content, string outputFile, string pathLESS)
        {
            try
            {
                Directory.SetCurrentDirectory(App.MapPath(pathLESS));
                var file = App.MapPath(outputFile);
                var dir = file.Replace(file.GetFilename(), "");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var css = Less.Parse(content);
                File.WriteAllText(file, css);
                Directory.SetCurrentDirectory(App.MapPath("/"));
            }
            catch (Exception ex)
            {
                throw new ServiceErrorException("Error generating compiled LESS resource");
            }
        }

        public static void CopyTempWebsite()
        {
            if (!File.Exists(App.MapPath("/Content/pages/home.html")))
            {
                //copy default website since none exists yet
                Directory.CreateDirectory(App.MapPath("/wwwroot/content/"));
                Directory.CreateDirectory(App.MapPath("/wwwroot/content/pages/"));
                Directory.CreateDirectory(App.MapPath("/wwwroot/fonts/"));
                Directory.CreateDirectory(App.MapPath("/wwwroot/images/"));
                Directory.CreateDirectory(App.MapPath("/wwwroot/js/"));
                Directory.CreateDirectory(App.MapPath("/wwwroot/css/"));
                Directory.CreateDirectory(App.MapPath("/Content/pages/"));
                Directory.CreateDirectory(App.MapPath("/Content/partials/"));
                Directory.CreateDirectory(App.MapPath("/Content/emails/"));

                //copy all temp folders into wwwroot
                var dir = new DirectoryInfo(App.MapPath("/Content/temp"));
                var exclude = new string[]
                {
                    "\\pages",
                    "\\partials",
                    "\\emails",
                    "\\app-css"
                };
                foreach (var d in dir.GetDirectories())
                {
                    if (!exclude.Any(a => d.FullName.Contains(a)))
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
                    var folder = App.MapPath("/wwwroot/content/" + file.Replace(contentFolder, "").Replace(filename, ""));
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                    var workingDir = file.Replace(contentFolder, "");
                    File.Copy(file, folder + filename, true);
                }
            }
        }

        public static class Settings
        {
            private static JsonSerializerOptions jsonOptions = new JsonSerializerOptions()
            {
                WriteIndented = true
            };

            public static Models.Website.Settings Load()
            {
                var file = App.MapPath("/Content/website.json");
                if (File.Exists(file))
                {
                    return JsonSerializer.Deserialize<Models.Website.Settings>(Cache.LoadFile(file));
                }
                return new Models.Website.Settings();
            }

            public static void Save(Models.Website.Settings settings)
            {
                var file = App.MapPath("/Content/website.json");
                File.WriteAllText(file, JsonSerializer.Serialize(settings, jsonOptions));
                Cache.Remove(file);
            }
        }
    }
}
