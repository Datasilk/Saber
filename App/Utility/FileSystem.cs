using System;
using System.Collections.Generic;
using System.IO;

namespace Utility
{
    public class FileSystem
    {

        public static void CopyDirectoryContents(string targetFolder, string outputFolder)
        {
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
                try
                {
                    File.Copy(path, path.Replace(targetFolder, outputFolder), true);
                }
                catch (Exception) { }
            }
        }
    }
}
