﻿using System.Linq;
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
                    user = Saber.User.Get(Context, Session);
                }
                return user;
            }
            set { user = value; }
        }

        public override bool CheckSecurity(string key = "")
        {
            if(User.IsAdmin) { return true; }
            if(key != "" && User.UserId > 0 && !User.Keys.Any(a => a.Key == key && a.Value == true))
            {
                return false;
            }else if(User.UserId <= 0)
            {
                return false;
            }
            return true;
        }

        public override string Render(string body = "")
        {
            var view = new View("/Views/Shared/layout.html");
            view["title"] = Title;
            view["description"] = Description;
            view["language"] = User.Language;
            view["theme"] = Theme;
            view["head-css"] = Css.ToString();
            view["footer"] = Footer != null ? Footer.ToString() : "";

            //load apple icons
            var appleIcons = new bool[4];
            var isCached = false;
            var i = 0;
            if (Cache.Store.ContainsKey("apple-icons"))
            {
                appleIcons = (bool[])Cache.Store["apple-icons"];
                isCached = true;
            }
            foreach (var size in new int[] { 60, 76, 120, 152 })
            {
                if (isCached == false && File.Exists(App.MapPath("/images/mobile/apple-" + size + "x" + size + ".png")))
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
                Cache.Add("apple-icons", appleIcons);
            }

            //load android icons
            if (Cache.Store.ContainsKey(AndroidKey))
            {
                if((bool)Cache.Store[AndroidKey] == true)
                {
                    view.Show(AndroidKey);
                }
            }else if (File.Exists(App.MapPath("/wwwroot/" + AndroidKey + ".json")))
            {
                Cache.Add(AndroidKey, true);
            }
            else
            {
                Cache.Add(AndroidKey, false);
            }

            //load body
            view["body"] = body;

            //add initialization script
            view["scripts"] = Scripts.ToString();

            return Common.Platform.Render.View(this, view);
        }

        public override string AccessDenied()
        {
            return AccessDenied<Controllers.Login>();
        }
    }
}