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
            if (Vendors.Controllers.ContainsKey(name))
            {
                //load Vendor controller
                return (IController)Activator.CreateInstance(Vendors.Controllers[name]);
            }
            //if all else fails, render Saber Editor
            return new Controllers.Editor();
        }
    }
}
