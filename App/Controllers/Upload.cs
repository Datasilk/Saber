using System;
using System.IO;
using System.Threading;
using Saber.Common.Platform;
using Saber.Common.Utility;
using Saber.Common.Extensions.Strings;

namespace Saber.Controllers
{
    public class Upload : Controller
    {
        private string thumbdir = "_thumbs/";

        public override string Render(string body = "")
        {
            if (!CheckSecurity()) { return AccessDenied<Login>(); }
            if (Parameters.Files.Count > 0 && Parameters.ContainsKey("path"))
            {
                //save resources for page
                var paths = PageInfo.GetRelativePath(Parameters["path"].ToString());
                var dir = string.Join("/", paths) + "/";
                var pubdir = dir; //published directory
                if (paths[0].ToLower() == "/content/pages")
                {
                    //loading resources for specific page
                    pubdir = "/wwwroot" + dir;
                }
                if (!Directory.Exists(Server.MapPath(pubdir)))
                {
                    Directory.CreateDirectory(Server.MapPath(pubdir));
                }
                foreach(var f in Parameters.Files)
                {
                    var file = Parameters.Files[f.Key];
                    var filename = file.Filename;
                    var ext = filename.GetFileExtension().ToLower();
                    try
                    {
                        using (var ms = new FileStream(Server.MapPath(pubdir + filename), FileMode.Create))
                        {
                            file.CopyTo(ms);
                            ms.Close();
                        }
                    }catch(Exception){}
                    var i = 0;
                    while (!File.Exists(Server.MapPath(pubdir + filename)) && i < 5)
                    {
                        i++;
                        Thread.Sleep(1000);
                    }
                    if(!File.Exists(Server.MapPath(pubdir + filename))) { return Error(); }

                    switch (ext)
                    {
                        case "jpg": case "jpeg": case "png":
                            //create a thumbnail image to display in the page resources section of the editor
                            if (!Directory.Exists(Server.MapPath(pubdir + thumbdir)))
                            {
                                Directory.CreateDirectory(Server.MapPath(pubdir + thumbdir));
                            }
                            if(File.Exists(Server.MapPath(pubdir + filename)))
                            {
                                try
                                {
                                    Image.Shrink(pubdir + filename, pubdir + thumbdir + filename, 480);
                                }
                                catch(Exception ex)
                                {
                                    return Error("An error occured when trying to create a thumbnail preview of your image upload");
                                }
                            }
                            break;
                    }
                }
            }
            return "";
        }
    }
}
