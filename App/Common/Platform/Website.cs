﻿using System;
using System.IO;
using System.Linq;
using Utility.Strings;
using dotless.Core;

namespace Saber.Common.Platform
{
    public static class Website
    {
        public static void NewFile(string path, string filename)
        {
            var server = Server.Instance;

            //check for root & content folders
            if (path == "root")
            {
                throw new ServiceErrorException("You cannot create a file in the root folder");
            }
            if (path.IndexOf("content") == 0)
            {
                throw new ServiceErrorException("You cannot create a file in the content folder");
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

            if (!Directory.Exists(Server.MapPath(dir)))
            {
                Directory.CreateDirectory(Server.MapPath(dir));
            }
            if (File.Exists(Server.MapPath(dir + filename.Replace(" ", ""))))
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
                    content = "body {\n\n}";
                    break;
                case "html":
                    content = "<p></p>";
                    break;
            }
            try
            {
                File.WriteAllText(Server.MapPath(dir + filename.Replace(" ", "")), content);
            }
            catch (Exception)
            {
                throw new ServiceErrorException("Error creating file");
            }
        }

        public static void NewFolder(string path, string folder)
        {
            var server = Server.Instance;

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

            if (!Directory.Exists(Server.MapPath(dir)))
            {
                try
                {
                    Directory.CreateDirectory(Server.MapPath(dir));
                }
                catch (Exception)
                {
                    throw new ServiceErrorException("Error creating new folder");
                }
            }
        }

        public static void SaveFile(string path, string content)
        {
            var server = Server.Instance;

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
            if (!Directory.Exists(Server.MapPath(dir)))
            {
                try
                {
                    Directory.CreateDirectory(Server.MapPath(dir));
                }
                catch (Exception)
                {
                    throw new ServiceErrorException("Error creating folder for file");
                }
            }
            try
            {
                File.WriteAllText(Server.MapPath(filepath), content);
            }
            catch (Exception)
            {
                throw new ServiceErrorException("Error writing to file");
            }

            //clean cache related to file
            switch (ext)
            {
                case "html":
                    //remove cached scaffold object
                    server.Scaffold.Remove(path);
                    break;
            }

            //process saved files
            if (paths[0].ToLower() == "/content/pages")
            {
                var pubdir = "/wwwroot/content/pages/" + string.Join("/", paths.Skip(1)).Replace(file, "");
                if(pubdir[pubdir.Length - 1] != '/') { pubdir += "/"; }
                switch (ext)
                {
                    case "js": case "css": case "less":
                        //create public folder in wwwroot
                        if (!Directory.Exists(Server.MapPath(pubdir)))
                        {
                            Directory.CreateDirectory(Server.MapPath(pubdir));
                        }
                        break;
                }

                switch (ext)
                {
                    case "js": case "css":
                        //copy resource file to public wwwroot folder
                        File.Copy(Server.MapPath(filepath), Server.MapPath(pubdir + file), true);
                        break;

                    case "less":
                        //compile less file
                        try
                        {
                            var css = Less.Parse(content);
                            File.WriteAllText(Server.MapPath(pubdir + file.Replace(".less", ".css")), css);

                            //use Gulp to compile JS, CSS, & LESS
                            //var p = new Process
                            //{
                            //    StartInfo = new ProcessStartInfo()
                            //    {
                            //        FileName = "cmd.exe",
                            //        Arguments = "/c gulp file --path \"" + string.Join("/", paths).ToLower().Substring(1) + "\"",
                            //        WindowStyle = ProcessWindowStyle.Hidden,
                            //        UseShellExecute = false,
                            //        CreateNoWindow = true,
                            //        RedirectStandardError = true,
                            //        WorkingDirectory = Server.MapPath("/").Replace("App\\", ""),
                            //        Verb = "runas"
                            //    }
                            //};
                            //p.OutputDataReceived += ProcessInfo.Gulp.OutputReceived;
                            //p.ErrorDataReceived += ProcessInfo.Gulp.ErrorReceived;
                            //p.Start();
                            //p.WaitForExit();
                        }
                        catch (Exception)
                        {
                            throw new ServiceErrorException("Error generating compiled resource");
                        }
                        break;
                }
            }
        }
    }
}
