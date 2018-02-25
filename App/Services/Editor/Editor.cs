using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Saber.Services
{
    public class Editor : Service
    {
        public Editor(Core DatasilkCore) : base(DatasilkCore)
        {
        }

        public string Dir(string path)
        {
            if (!CheckSecurity()) { return AccessDenied(); }

            var paths = path.Split('/');
            var rpath = "";
            var rid = string.Join("_", paths);
            var html = new StringBuilder();

            //translate root path to relative path
            switch (paths[0].ToLower())
            {
                case "root": paths[0] = ""; break;
                case "css": paths[0] = "/CSS/"; break;
                case "pages": paths[0] = "/Pages/"; break;
                case "partials": paths[0] = "/Partials/"; break;
                case "scripts": paths[0] = "/Scripts/"; break;
                case "services": paths[0] = "/Services/"; break;
                case "content": paths[0] = "/Content/pages/"; break;
                default: return Error();
            }
            rpath = string.Join("/", path) + "/";

            var item = new Scaffold("/Services/Editor/file.html", S.Server.Scaffold);
            var items = new List<string>();

            if(paths[0] == "")
            {
                //display root folders for website
                items = new List<string>()
                {
                    "CSS", "Pages", "Partials", "Scripts", "Services", "access-denied.html", "layout.html"
                };
            }
            else
            {
                //get folder structure from hard drive
                var info = new DirectoryInfo(S.Server.MapPath(rpath));
                foreach(var dir in info.GetDirectories())
                {
                    if(dir.Name.IndexOfAny(new char[] { '.', '_' }) != 0)
                    {
                        items.Add(dir.Name);
                    }
                }
                foreach(var file in info.GetFiles())
                {
                    switch(file.Name.Split(".", 2)[1].ToLower())
                    {
                        case "html": case "css": case "less": case "js":
                            items.Add(file.Name); break;
                    }
                }
            }
            foreach(var i in items)
            {
                if(i.IndexOf(".") > 0)
                {
                    item.Data["icon"] =  "file-" + i.Split('.', 2)[1].ToLower();
                }
                else
                {
                    item.Data["icon"] = "folder";
                }
                item.Data["id"] = rid + "_" + i.Replace(".", "_");
                item.Data["path"] = rid.Replace("_", "_") + "/" + i;
                item.Data["title"] = i;
                html.Append(item.Render());
            }
            return html.ToString();
        }
    }
}
