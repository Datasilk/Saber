using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Saber.Common.Extensions.Strings;
using dotless.Core;


namespace Saber.Common.Platform
{
    public static class Website
    {
        public static void NewFile(string path, string filename)
        {
            //check for root & content folders
            if (path == "root")
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
            if (path == "root")
            {
                throw new ServiceErrorException("You cannot create a file in the root folder");
            }
            if (path.IndexOf("content") == 0)
            {
                throw new ServiceErrorException("You cannot create a file in the content folder");
            }

            //check folder characters
            if (!folder.OnlyLettersAndNumbers(new string[] { "-", "_" }))
            {
                throw new ServiceErrorException("Folder must be alpha-numeric and may contain dashes - and underscores _");
            }

            var paths = PageInfo.GetRelativePath(path.ToLower());
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
            var filepath = string.Join("/", paths); //relative filename & path
            var file = paths[paths.Length - 1]; //filename only
            var ext = file.Split('.', 2)[1].ToLower(); //file extension only

            //create folder for file
            if (!Directory.Exists(App.MapPath(dir)))
            {
                try
                {
                    Directory.CreateDirectory(App.MapPath(dir));
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
                                SaveLessFile(File.ReadAllText(App.MapPath("/CSS/website.less")), "/wwwroot/css/website.css", "/CSS");
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
                }
            }
            else if (paths[0].ToLower() == "/css")
            {
                switch (paths[1].ToLower())
                {
                    case "website.less":
                        SaveLessFile(content, "/wwwroot/css/website.css", "/CSS");
                        break;
                }
            }
        }

        public static void SaveLessFile(string content, string outputFile, string pathLESS)
        {
            try
            {
                Directory.SetCurrentDirectory(App.MapPath(pathLESS));
                var css = Less.Parse(content);
                File.WriteAllText(App.MapPath(outputFile), css);
                Directory.SetCurrentDirectory(App.MapPath("/"));
            }
            catch (Exception)
            {
                throw new ServiceErrorException("Error generating compiled LESS resource");
            }
        }

        public static List<string> AllFiles(string[] include = null)
        {
            return Core.Website.AllFiles(include);
        }

        public static void ResetCache(string path, string language = "en")
        {
            var paths = PageInfo.GetRelativePath(path);
            var filepath = string.Join("/", paths);
            Cache.Remove(ContentFields.ContentFile(path, language));
            ViewCache.Remove(filepath + ".html");
        }

        private static void RecurseDirectories(List<string> list, string path, string[] ignore = null)
        {
            var parent = new DirectoryInfo(App.MapPath(path));
            var dirs = parent.GetDirectories().Where(a => ignore != null ? ignore.Where(b => a.FullName.IndexOf(b) >= 0).Count() == 0 : true);
            list.AddRange(parent.GetFiles().Select(a => a.FullName).Where(a => ignore != null ? ignore.Where(b => a.IndexOf(b) >= 0).Count() == 0  : true));
            foreach(var dir in dirs)
            {
                var subpath = dir.FullName;
                if (App.IsDocker)
                {
                    subpath = "/" + subpath.Split("/app/")[1];
                }
                else
                {
                    subpath = "\\" + subpath.Split("\\App\\")[1];
                }
                RecurseDirectories(list, subpath, ignore);
            }
        }
    }
}
