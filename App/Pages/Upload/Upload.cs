using System;
using System.IO;
using System.Threading;

namespace Saber.Pages
{
    public class Upload : Page
    {
        private string thumbdir = "_thumbs/";

        public Upload(Core DatasilkCore) : base(DatasilkCore)
        {
        }

        public override string Render(string[] path, string body = "", object metadata = null)
        {
            if (!CheckSecurity()) { return AccessDenied(true, new Login(S)); }
            if (Files.Count > 0 && Form.ContainsKey("path"))
            {
                //save resources for page
                var paths = Utility.Page.GetRelativePath(Form["path"].ToString());
                var dir = string.Join("/", paths) + "/";
                var pubdir = dir; //published directory
                if (paths[0].ToLower() == "/content/pages")
                {
                    //loading resources for specific page
                    pubdir = "/wwwroot" + dir;
                }
                if (!Directory.Exists(S.Server.MapPath(pubdir)))
                {
                    Directory.CreateDirectory(S.Server.MapPath(pubdir));
                }
                foreach(var file in Files)
                {
                    var filename = file.FileName;
                    var ext = S.Util.Str.getFileExtension(filename).ToLower();
                    try
                    {
                        var ms = new FileStream(S.Server.MapPath(pubdir + filename), FileMode.Create);
                        file.CopyTo(ms);
                        ms.Close();
                        ms.Dispose();
                    }catch(Exception ex)
                    {

                    }
                    var i = 0;
                    while (!File.Exists(S.Server.MapPath(pubdir + filename)) && i < 5)
                    {
                        i++;
                        Thread.Sleep(1000);
                    }
                    if(!File.Exists(S.Server.MapPath(pubdir + filename))) { return Error(); }

                    switch (ext)
                    {
                        case "jpg": case "jpeg": case "png":
                            //create a thumbnail image to display in the page resources section of the editor
                            var img = new global::Utility.Images(S);
                            if (!Directory.Exists(S.Server.MapPath(pubdir + thumbdir)))
                            {
                                Directory.CreateDirectory(S.Server.MapPath(pubdir + thumbdir));
                            }
                            if(File.Exists(S.Server.MapPath(pubdir + filename)))
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
