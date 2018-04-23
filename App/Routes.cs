using Microsoft.AspNetCore.Http;
using Datasilk;

public class Routes : Datasilk.Routes
{
    public Routes(HttpContext context) : base(context) { }

    public override Page FromPageRoutes(string name)
    {
        switch (name)
        {
            case "login": return new Saber.Pages.Login(context);
            case "upload": return new Saber.Pages.Upload(context);
            default: return new Saber.Pages.Editor(context);
        }

    }

    public override Service FromServiceRoutes(string name)
    {
        return null;
    }
}
