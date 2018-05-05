using Microsoft.AspNetCore.Http;
using Datasilk;

public class Routes : Datasilk.Routes
{
    public override Page FromPageRoutes(HttpContext context, string name)
    {
        switch (name)
        {
            case "login": return new Saber.Pages.Login(context);
            case "upload": return new Saber.Pages.Upload(context);
            default: return new Saber.Pages.Editor(context);
        }
    }

    public override Service FromServiceRoutes(HttpContext context, string name)
    {
        return null;
    }
}
