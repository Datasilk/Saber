using Microsoft.AspNetCore.Http;
using Datasilk.Mvc;
using Saber.Pages;

public class Routes : Datasilk.Web.Routes
{
    public override Controller FromControllerRoutes(HttpContext context, Parameters parameters, string name)
    {
        switch (name)
        {
            case "login":
                if (Server.hasAdmin == false || Server.resetPass == true)
                {
                    return new Login(context, parameters);
                }
                else
                {
                    return new Editor(context, parameters);
                }
            case "logout": return new Logout(context, parameters);
            case "upload": return new Upload(context, parameters);
            default: return new Editor(context, parameters);
        }
    }
}
