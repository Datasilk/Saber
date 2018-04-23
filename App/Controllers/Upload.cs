using System;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Utility.Strings;
using Saber.Common;

namespace Saber.Pages
{
    public class Upload : Page
    {
        private string thumbdir = "_thumbs/";

        public Upload(HttpContext context) : base(context)
        {
        }

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            if (!CheckSecurity()) { return AccessDenied(true, new Login(context)); }
            if (context.Request.Form.Files.Count > 0 && context.Request.Form.ContainsKey("path"))
            {
                //save resources for page
                var paths = PageInfo.GetRelativePath(context.Request.Form["path"].ToString());
                var dir = string.Join("/", paths) + "/";
                var pubdir = dir; //published directory
                if (paths[0].ToLower() == "/content/pages")
                {
                    //loading resources for specific page
                    pubdir = "/wwwroot" + dir;
                }
                if (!Directory.Exists(server.MapPath(pubdir)))
                {
                    Directory.CreateDirectory(server.MapPath(pubdir));
                }
                foreach(var file in context.Request.Form.Files)
                {
                    var filename = file.FileName;
                    var ext = filename.GetFileExtension().ToLower();
                    try
                    {
                        var ms = new FileStream(server.MapPath(pubdir + filename), FileMode.Create);
                        file.CopyTo(ms);
                        ms.Close();
                        ms.Dispose();
                    }catch(Exception ex)
                    {

                    }
                    var i = 0;
                    while (!File.Exists(server.MapPath(pubdir + filename)) && i < 5)
                    {
                        i++;
                        Thread.Sleep(1000);
                    }
                    if(!File.Exists(server.MapPath(pubdir + filename))) { return Error(); }

                    switch (ext)
                    {
                        case "jpg": case "jpeg": case "png":
                            //create a thumbnail image to display in the page resources section of the editor
                            var img = new Common.Images();
                            if (!Directory.Exists(server.MapPath(pubdir + thumbdir)))
                            {
                                Directory.CreateDirectory(server.MapPath(pubdir + thumbdir));
                            }
                            if(File.Exists(server.MapPath(pubdir + filename)))
                            {
                                img.Shrink(pubdir + filename, pubdir + thumbdir + filename, 480);
                            }
                            break;
                    }
                }
            }
            return "";
        }
    }
}
