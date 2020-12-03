using System;
using System.IO;
using System.Linq;
using Saber.Core.Extensions.Strings;

namespace Saber.Common.Utility
{
    public static class FileSystem
    {

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
                catch (Exception) { }
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
                catch (Exception) { }
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
