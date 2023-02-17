using System;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using Saber.Core.Extensions.Strings;

namespace Saber.Common.Utility
{
    public static class FileSystem
    {
        /// <summary>
        /// Create a directory within the file system
        /// </summary>
        /// <param name="path">The relative path</param>
        public static void CreateDirectory(string path)
        {
            try
            {
                Directory.CreateDirectory(App.MapPath(path));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }
        }

        public static void CopyDirectoryContents(string targetFolder, string outputFolder, string[] extensions = null)
        {
            if(extensions == null) { extensions = new string[] { }; }
            //first, copy all sub directories
            foreach (var path in Directory.GetDirectories(targetFolder, "*", SearchOption.AllDirectories))
            {
                try
                {
                    Directory.CreateDirectory(path.Replace(targetFolder, outputFolder));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                }
            }

            //next, copy all files in sub directories
            foreach (var path in Directory.GetFiles(targetFolder, "*.*", SearchOption.AllDirectories))
            {
                var ext = "." + path.GetFileExtension();
                if (extensions.Length > 0 && !extensions.Contains(ext)) { continue; }
                try
                {
                    File.Copy(path, path.Replace(targetFolder, outputFolder), true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                }
            }
        }

        public static string[] GetAllFiles(string targetFolder, bool recurseFolders = true, string filePattern = "*.*", string[] exclude = null)
        {
            if(exclude == null) { exclude = new string[] { }; }
            exclude = exclude.Select(a => a.Replace("\\", "/")).ToArray();
            return Directory.GetFiles(App.MapPath(targetFolder), filePattern, recurseFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Where(a =>
                {
                    var a2 = a.Replace("\\", "/");
                    return !exclude.Any(b => a2.Contains(b));
                }).ToArray();
        }
    }
}
