using System.IO;
using Saber.Core;

namespace Saber
{

    public class Controller : Core.Controller
    {
        private string AndroidKey = "android-manifest";

        public override IUser User
        {
            get
            {
                if (user == null)
                {
                    user = Saber.User.Get(Context);
                }
                return user;
            }
            set { user = value; }
        }

        public override string Render(string body = "")
        {
            if (UsePlatform == true)
            {
                Scripts.Append("<script language=\"javascript\">" + 
                    "S.svg.load('/editor/icons.svg');" + 
                    "S.svg.load('/editor/loader.svg');" + 
                    "</script>");
            }
            var view = new View("/Views/Shared/layout.html");
            view["title"] = Title;
            view["description"] = Description;
            view["language"] = User.Language;
            view["theme"] = Theme;
            view["head-css"] = Css.ToString();
            view["footer"] = Footer != null ? Footer.ToString() : "";

            //load website icon
            if (File.Exists(Server.MapPath("wwwroot/images/web-icon.png")))
            {
                view["favicon"] = "/images/web-icon.png";
                view["favicon-type"] = "image/png";
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
            if (Server.Cache.ContainsKey(AndroidKey))
            {
                if((bool)Server.Cache[AndroidKey] == true)
                {
                    view.Show(AndroidKey);
                }
            }else if (File.Exists(Server.MapPath("/wwwroot/" + AndroidKey + ".json")))
            {
                Server.Cache.Add(AndroidKey, true);
            }
            else
            {
                Server.Cache.Add(AndroidKey, false);
            }

            //load body
            view["body"] = body;
            if (UsePlatform)
            {
                view.Show("platform-1");
                view.Show("platform-2");
                view.Show("platform-3");
            }

            //add initialization script
            view["scripts"] = Scripts.ToString();

            return view.Render();
        }
    }
}