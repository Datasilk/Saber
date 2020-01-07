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
            if (Context.Request.Form.Files.Count > 0 && Context.Request.Form.ContainsKey("path"))
            {
                //save resources for page
                var paths = PageInfo.GetRelativePath(Context.Request.Form["path"].ToString());
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
                foreach(var file in Context.Request.Form.Files)
                {
                    var filename = file.FileName;
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
                                Image.Shrink(pubdir + filename, pubdir + thumbdir + filename, 480);
                            }
                            break;
                    }
                }
            }
            return "";
        }
    }
}
