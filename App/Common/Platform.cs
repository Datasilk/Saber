using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Utility.Serialization;
using Utility.Strings;
using CommonMark;

namespace Saber.Common
{
    public static class Platform
    {

        #region "Render Page"
        /// <summary>
        /// Renders a page, including language-specific content, and uses page-specific config to check for security & other features
        /// </summary>
        /// <param name="path">relative path to content (e.g. "content/home")</param>
        /// <returns>rendered HTML of the page content (not including any layout, header, or footer)</returns>
        public static string RenderPage(string path, Datasilk.Request request, User user)
        {
            //translate root path to relative path
            var server = Server.Instance;
            var paths = PageInfo.GetRelativePath(path);
            var relpath = string.Join("/", paths);
            var file = paths[paths.Length - 1];
            var fileparts = file.Split(".", 2);
            if (paths.Length == 0) {
                throw new ServiceErrorException("No path specified");
            }

            //check file path on drive for (estimated) OS folder structure limitations 
            if (server.MapPath(relpath).Length > 180)
            {
                throw new ServiceErrorException("The URL path you are accessing is too long to handle for the web server");
            }
            var scaffold = new Scaffold(relpath, server.Scaffold);
            if (scaffold.elements.Count == 0)
            {
                if (request.User.userId == 0)
                {
                    scaffold.HTML = "<p>This page does not exist. Please log into your account to write content for this page.</p>";
                }
                else
                {
                    scaffold.HTML = "<p>Write content using HTML & CSS</p>";
                }
            }

            //load user content from json file, depending on selected language
            var config = PageInfo.GetPageConfig(path);
            var lang = user.language;

            //check security
            if (config.security.secure == true)
            {
                if (!request.CheckSecurity() || !config.security.read.Contains(user.userId))
                {
                    throw new ServiceDeniedException("You do not have read access for this page");
                }
            }

            var contentfile = ContentFile(path, lang);
            var data = (Dictionary<string, string>)Serializer.ReadObject(server.LoadFileFromCache(contentfile, true), typeof(Dictionary<string, string>));
            if (data != null)
            {
                foreach (var item in data)
                {
                    if (item.Value.IndexOf("\n") >= 0)
                    {
                        scaffold.Data[item.Key] = CommonMarkConverter.Convert(item.Value);
                    }
                    else
                    {
                        scaffold.Data[item.Key] = item.Value;
                    }
                }
            }
            return scaffold.Render();
        }
        #endregion

        #region "File System"
        public static void SaveFile(string path, string content)
        {
            var server = Server.Instance;
            var paths = PageInfo.GetRelativePath(path);
            if (paths.Length == 0)
            {
                throw new ServiceErrorException("No path specified");
            }
            
            var dir = string.Join("/", paths.Take(paths.Length - 1));
            dir = server.MapPath(dir);
            var filepath = server.MapPath(string.Join("/", paths));
            if (!Directory.Exists(dir))
            {
                try
                {
                    Directory.CreateDirectory(dir);
                }
                catch (Exception)
                {
                    throw new ServiceErrorException("Error creating folder for file");
                }
            }
            try
            {
                File.WriteAllText(filepath, content);
            }
            catch (Exception)
            {
                throw new ServiceErrorException("Error writing to file");
            }

            //clean cache related to file
            var file = paths[paths.Length - 1];
            var ext = file.Split('.', 2)[1].ToLower();
            switch (ext)
            {
                case "html":
                    //remove cached scaffold object
                    server.Scaffold.Remove(path);
                    break;
            }

            //gulp file
            if (paths[0].ToLower() == "/content/pages")
            {
                switch (ext)
                {
                    case "js":
                    case "css":
                    case "less":
                        try
                        {
                            var p = new Process
                            {
                                StartInfo = new ProcessStartInfo()
                                {
                                    FileName = "cmd.exe",
                                    Arguments = "/c gulp file --path \"" + string.Join("/", paths).ToLower().Substring(1) + "\"",
                                    WindowStyle = ProcessWindowStyle.Hidden,
                                    UseShellExecute = false,
                                    CreateNoWindow = true,
                                    RedirectStandardError = true,
                                    WorkingDirectory = server.MapPath("/").Replace("App\\", ""),
                                    Verb = "runas"
                                }
                            };
                            p.OutputDataReceived += GulpOutputReceived;
                            p.ErrorDataReceived += GulpErrorReceived;
                            p.Start();
                        }
                        catch (Exception)
                        {
                            throw new ServiceErrorException("Error creating thumbnail image");
                        }
                        break;
                }
            }
        }
        
        private static void GulpOutputReceived(object sender, DataReceivedEventArgs e)
        {
            Process p = sender as Process;
            if (p == null) { return; }
            //Console.WriteLine(e.Data);
        }

        private static void GulpErrorReceived(object sender, DataReceivedEventArgs e)
        {
            Process p = sender as Process;
            if (p == null) { return; }
            //Console.WriteLine(e.Data);
        }

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

            if (!Directory.Exists(server.MapPath(dir)))
            {
                Directory.CreateDirectory(server.MapPath(dir));
            }
            if (File.Exists(server.MapPath(dir + filename.Replace(" ", ""))))
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
                File.WriteAllText(server.MapPath(dir + filename.Replace(" ", "")), content);
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

            if (!Directory.Exists(server.MapPath(dir)))
            {
                try
                {
                    Directory.CreateDirectory(server.MapPath(dir));
                }
                catch (Exception)
                {
                    throw new ServiceErrorException("Error creating new folder");
                }
            }
        }
        #endregion

        #region "Content Fields"
        public static string ContentFile(string path, string language)
        {
            var paths = PageInfo.GetRelativePath(path);
            var relpath = string.Join("/", paths);
            var file = paths[paths.Length - 1];
            var fileparts = file.Split(".", 2);
            return relpath.Replace(file, fileparts[0] + "_" + language + ".json");
        }

        public static Dictionary<string, string> GetPageContent(string path, string language)
        {
            var server = Server.Instance;
            var contentfile = server.MapPath(ContentFile(path, language));
            var content = (Dictionary<string, string>)Serializer.ReadObject(server.LoadFileFromCache(contentfile, true), typeof(Dictionary<string, string>));
            if (content != null) { return content; }
            return new Dictionary<string, string>();
        }
        #endregion
    }
}
