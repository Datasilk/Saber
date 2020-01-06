using System.IO;
using Datasilk.Core.Web;

namespace Saber
{
    public enum EditorType
    {
        Monaco = 0,
        Ace = 1
    }

    public class Controller : Request, IController
    {

        public bool usePlatform = false;
        public string title = "Datasilk";
        public string description = "";
        public string theme = "default";
        private string androidKey = "android-manifest";

        public EditorType EditorUsed
        {
            get { return EditorType.Monaco; }
        }

        public virtual string Render(string body = "")
        {
            if (usePlatform == true)
            {
                Scripts.Append("<script language=\"javascript\">S.svg.load('/editor/icons.svg');</script>");
            }
            var view = new View("/Views/Shared/layout.html");
            view["title"] = title;
            view["description"] = description;
            view["language"] = User.language;
            view["theme"] = theme;
            view["head-css"] = Css.ToString();

            //load website icon
            if (File.Exists(Server.MapPath("/images/favicon.png")))
            {
                view["favicon"] = "/images/favicon.png";
                view["favicon-type"] = "image/png";
            }
            else
            {
                view["favicon"] = "/images/favicon.ico";
                view["favicon-type"] = "image/ico";
            }

            //load apple icons
            var appleIcons = new bool[4];
            var isCached = false;
            var i = 0;
            if (Server.Cache.ContainsKey("apple-icons"))
            {
                appleIcons = (bool[])Server.Cache["apple-icons"];
                isCached = true;
            }
            foreach (var size in new int[] { 60, 76, 120, 152 })
            {
                if (isCached == false && File.Exists(Server.MapPath("/images/mobile/apple-" + size + "x" + size + ".png")))
                {
                    appleIcons[i] = true;
                }
                else
                {
                    appleIcons[i] = false;
                }
                view.Show("apple-app-" + size);
                view.Show("apple-app");
                i++;
            }
            if(isCached == false)
            {
                Server.Cache.Add("apple-icons", appleIcons);
            }

            //load android icons
            if (Server.Cache.ContainsKey(androidKey))
            {
                if((bool)Server.Cache[androidKey] == true)
                {
                    view.Show(androidKey);
                }
            }else if (File.Exists(Server.MapPath("/wwwroot/" + androidKey + ".json")))
            {
                Server.Cache.Add(androidKey, true);
            }
            else
            {
                Server.Cache.Add(androidKey, false);
            }

            //load body
            view["body"] = body;
            if (usePlatform)
            {
                view.Show("platform-1");
                view.Show("platform-2");
                view.Show("platform-3");
            }

            //add initialization script
            view["scripts"] = Scripts.ToString();

            return view.Render();
        }

        public override void Unload()
        {
            User.Save();
        }

        public override bool CheckSecurity()
        {
            if (!base.CheckSecurity()) { 
                return false; 
            }
            if (User.userId > 0)
            {
                return true;
            }
            return false;
        }

        public string AccessDenied<T>() where T : IController
        {
            return IController.AccessDenied<T>();
        }

        public string Redirect(string url)
        {
            return "<script language=\"javascript\">window.location.href = '" + url + "';</script>";
        }

        public override void AddScript(string url, string id = "", string callback = "")
        {
            if (ContainsResource(url)) { return; }
            Scripts.Append("<script language=\"javascript\"" + (id != "" ? " id=\"" + id + "\"" : "") + " src=\"" + url + "\"" +
                (callback != "" ? " onload=\"" + callback + "\"" : "") + "></script>");
        }

        public override void AddCSS(string url, string id = "")
        {
            if (ContainsResource(url)) { return; }
            Css.Append("<link rel=\"stylesheet\" type=\"text/css\"" + (id != "" ? " id=\"" + id + "\"" : "") + " href=\"" + url + "\"></link>");
        }

        public bool ContainsResource(string url)
        {
            if (Resources.Contains(url)) { return true; }
            Resources.Add(url);
            return false;
        }
    }
}