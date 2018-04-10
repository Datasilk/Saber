using System;
using System.IO;

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
                var dir = string.Join("/", paths) + "/";
                if (!Directory.Exists(S.Server.MapPath(dir)))
                {
                    Directory.CreateDirectory(S.Server.MapPath(dir));
                }
                foreach(var file in Files)
                {
                    var filename = file.FileName;
                    var ms = new MemoryStream();
                    file.CopyTo(ms);
                    ms.Position = 0;
                    var sr = new StreamReader(ms);
                    var txt = sr.ReadToEnd();
                    File.WriteAllText(S.Server.MapPath(dir + filename), txt);
                }
            }
            return "";
        }
    }
}
