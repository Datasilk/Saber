using System;
using Microsoft.AspNetCore.Http;
using Datasilk.Core.Web;

namespace Saber
{
    public class Routes : Datasilk.Core.Web.Routes
    {
        public override IController FromControllerRoutes(HttpContext context, Parameters parameters, string name)
        {
            if(App.Environment == Environment.development) { ViewCache.Clear(); }
            switch (name)
            {
                case "editor": return new Controllers.Editor();
                case "login":
                    if (Server.HasAdmin == false || Server.ResetPass == true || parameters.ContainsKey("client_id"))
                    {
                        //display internal login page for admin account creation or OAuth 2.0 authentication
                        return new Controllers.Login();
                    }
                    else
                    {
                        return new Controllers.Page();
                    }
                case "logout": return new Controllers.Logout();
                case "upload": return new Controllers.Upload();
            }
            if (Core.Vendors.Controllers.ContainsKey(name))
            {
                //load Vendor controller
                return (IController)Activator.CreateInstance(Core.Vendors.Controllers[name]);
            }
            //if all else fails, render Saber Editor
            return new Controllers.Page();
        }

        public override IService FromServiceRoutes(HttpContext context, Parameters parameters, string name)
        {
            switch (name)
            {
                case "analytics": return new Services.Analytics();
                case "contentfields": return new Services.ContentFields();
                case "files": return new Services.Files();
                case "languages": return new Services.Languages();
                case "page": return new Services.Page();
                case "pageresources": return new Services.PageResources();
                case "pagesettings": return new Services.PageSettings();
                case "security": return new Services.Security();
                case "user": return new Services.User();
                case "users": return new Services.Users();
                case "websitesettings": return new Services.WebsiteSettings();
            }
            if (Core.Vendors.Services.ContainsKey(name))
            {
                //load Vendor service
                return (IService)Activator.CreateInstance(Core.Vendors.Services[name]);
            }
            return null;
        }
    }
}
