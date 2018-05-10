using Microsoft.AspNetCore.Http;
using Datasilk;

public class Routes : Datasilk.Routes
{
    public override Page FromPageRoutes(HttpContext context, string name)
    {
        Server Server = Server.Instance;
        switch (name)
        {
            case "login":
                if(Server.hasAdmin == false || Server.resetPass == true)
                {
                    return new Saber.Pages.Login(context);
                }
                else
                {
                    return new Saber.Pages.Editor(context);
                }
                
            case "upload": return new Saber.Pages.Upload(context);
            default: return new Saber.Pages.Editor(context);
        }
    }

    public override Service FromServiceRoutes(HttpContext context, string name)
    {
        return null;
    }
}
