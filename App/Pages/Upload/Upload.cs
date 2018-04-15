using System;
using System.IO;
using System.Threading;

namespace Saber.Pages
{
    public class Upload : Page
    {
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
                var dir = "/wwwroot/" + string.Join("/", paths) + "/";
                if (!Directory.Exists(S.Server.MapPath(dir)))
                {
                    Directory.CreateDirectory(S.Server.MapPath(dir));
                }
                foreach(var file in Files)
                {
                    var filename = file.FileName;
                    var ext = S.Util.Str.getFileExtension(filename).ToLower();
                    try
                    {
                        var ms = new FileStream(S.Server.MapPath(dir + filename), FileMode.Create);
                        file.CopyTo(ms);
                        ms.Close();
                        ms.Dispose();
                    }catch(Exception ex)
                    {

                    }
                    var i = 0;
                    while (!File.Exists(S.Server.MapPath(dir + filename)) && i < 5)
                    {
                        i++;
                        Thread.Sleep(1000);
                    }
                    if(!File.Exists(S.Server.MapPath(dir + filename))) { return Error(); }

                    switch (ext)
                    {
                        case "jpg": case "jpeg": case "png":
                            //create a thumbnail image to display in the page resources section of the editor
                            var img = new global::Utility.Images(S);
                            
                            if(File.Exists(S.Server.MapPath(dir + filename)))
                            {
                                img.Shrink(dir + filename, dir + filename.Replace("." + ext, "_sm." + ext), 480);
                            }
                            break;
                    }
                }
            }
            return "";
        }
    }
}
