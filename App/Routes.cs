using System;
using Microsoft.AspNetCore.Http;
using Datasilk.Core.Web;

namespace Saber
{
    public class Routes : Datasilk.Core.Web.Routes
    {
        public override IController FromControllerRoutes(HttpContext context, Parameters parameters, string name)
        {
            switch (name)
            {
                case "login":
                    if (Server.HasAdmin == false || Server.ResetPass == true)
                    {
                        return new Controllers.Login();
                    }
                    else
                    {
                        return new Controllers.Editor();
                    }
                case "logout": return new Controllers.Logout();
                case "upload": return new Controllers.Upload();
            }
            if (Common.Vendors.Controllers.ContainsKey(name))
            {
                //load Vendor controller
                return (IController)Activator.CreateInstance(Common.Vendors.Controllers[name]);
            }
            //if all else fails, render Saber Editor
            return new Controllers.Editor();
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
            if (Common.Vendors.Services.ContainsKey(name))
            {
                //load Vendor service
                return (IService)Activator.CreateInstance(Common.Vendors.Services[name]);
            }
            return null;
        }
    }
}
