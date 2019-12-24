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
                    if (Server.hasAdmin == false || Server.resetPass == true)
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
            if (Server.vendorControllers.ContainsKey(name))
            {
                //load Vendor controller
                return (Controller)Activator.CreateInstance(Server.vendorControllers[name]);
            }
            //if all else fails, render Saber Editor
            return new Controllers.Editor();
        }
    }
}
